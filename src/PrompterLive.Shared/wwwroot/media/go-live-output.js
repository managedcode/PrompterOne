(function () {
    const composerNamespace = "PrompterLiveGoLiveMediaComposer";
    const interopNamespace = "PrompterLiveGoLiveOutput";
    const supportNamespace = "PrompterLiveGoLiveOutputSupport";
    const outputSessions = new Map();
    const browserEnvironment = "browser";
    const liveKitAudioSource = "microphone";
    const liveKitAudioTrackName = "prompterlive-program-audio";
    const liveKitVideoSource = "camera";
    const liveKitVideoTrackName = "prompterlive-program-video";
    const obsStudioEnvironment = "obsstudio";
    const primaryAudioElementPrefix = "prompterlive-go-live-obs-audio-";

    function getComposer() {
        const composer = window[composerNamespace];
        if (!composer?.ensureProgramSession) {
            throw new Error("Go Live media compositor runtime is not available.");
        }

        return composer;
    }

    function getSupport() {
        const support = window[supportNamespace];
        if (!support?.normalizeRequest) {
            throw new Error("Go Live output support runtime is not available.");
        }

        return support;
    }

    function getLiveKitClient() {
        const client = window.LivekitClient;
        if (!client?.Room) {
            throw new Error("LiveKit client runtime is not available.");
        }

        return client;
    }

    function getObsEnvironment() {
        return typeof window.obsstudio === "object" && window.obsstudio !== null
            ? obsStudioEnvironment
            : browserEnvironment;
    }

    function ensureSession(sessionId) {
        if (!outputSessions.has(sessionId)) {
            outputSessions.set(sessionId, {
                audioDeviceId: "",
                liveKitActive: false,
                liveKitConnected: false,
                liveKitPublishedAudioTrack: null,
                liveKitPublishedVideoTrack: null,
                liveKitRoom: null,
                liveKitRoomName: "",
                liveKitServerUrl: "",
                mediaRecorder: null,
                mediaStream: null,
                obsActive: false,
                obsAudioElementId: "",
                obsEnvironment: browserEnvironment,
                recordingActive: false,
                recordingChunks: [],
                recordingFileHandle: null,
                recordingFileName: "",
                recordingMimeType: "",
                recordingRequestedAudioCodec: "",
                recordingRequestedContainer: "",
                recordingRequestedVideoCodec: "",
                recordingSaveMode: "",
                recordingWritable: null,
                recordingWritePromise: Promise.resolve(),
                requestSnapshot: null,
                videoDeviceId: ""
            });
        }

        return outputSessions.get(sessionId);
    }

    async function republishLiveKitTracks(session, liveKitClient) {
        const room = session.liveKitRoom;
        if (!room?.localParticipant) {
            throw new Error("LiveKit room is not connected.");
        }

        if (session.liveKitPublishedVideoTrack) {
            await room.localParticipant.unpublishTrack(session.liveKitPublishedVideoTrack, false).catch(() => {});
            session.liveKitPublishedVideoTrack = null;
        }

        if (session.liveKitPublishedAudioTrack) {
            await room.localParticipant.unpublishTrack(session.liveKitPublishedAudioTrack, false).catch(() => {});
            session.liveKitPublishedAudioTrack = null;
        }

        const videoTrack = session.mediaStream?.getVideoTracks()?.[0] ?? null;
        const audioTrack = session.mediaStream?.getAudioTracks()?.[0] ?? null;

        if (videoTrack) {
            await room.localParticipant.publishTrack(videoTrack, {
                name: liveKitVideoTrackName,
                source: liveKitClient.Track?.Source?.Camera ?? liveKitVideoSource
            });
            session.liveKitPublishedVideoTrack = videoTrack;
        }

        if (audioTrack) {
            await room.localParticipant.publishTrack(audioTrack, {
                name: liveKitAudioTrackName,
                source: liveKitClient.Track?.Source?.Microphone ?? liveKitAudioSource
            });
            session.liveKitPublishedAudioTrack = audioTrack;
        }
    }

    async function detachObsAudio(session) {
        if (!session.obsAudioElementId) {
            return;
        }

        const element = document.getElementById(session.obsAudioElementId);
        if (element) {
            element.pause?.();
            element.srcObject = null;
            element.remove();
        }

        session.obsAudioElementId = "";
    }

    async function attachObsAudio(sessionId, session) {
        await detachObsAudio(session);
        session.obsEnvironment = getObsEnvironment();

        if (!session.mediaStream?.getAudioTracks()?.length || session.obsEnvironment !== obsStudioEnvironment) {
            return;
        }

        const elementId = `${primaryAudioElementPrefix}${sessionId}`;
        const audioElement = document.createElement("audio");
        audioElement.id = elementId;
        audioElement.autoplay = true;
        audioElement.hidden = true;
        audioElement.muted = false;
        audioElement.playsInline = true;
        audioElement.srcObject = session.mediaStream;
        document.body.appendChild(audioElement);
        await audioElement.play().catch(() => {});
        session.obsAudioElementId = elementId;
    }

    async function cleanupSessionIfIdle(sessionId, session) {
        if (session.liveKitActive || session.obsActive || session.recordingActive) {
            return;
        }

        await detachObsAudio(session);
        await getComposer().cleanupProgramSession(session);
        outputSessions.delete(sessionId);
    }

    async function ensureProgramSession(session, rawRequest) {
        const request = getSupport().normalizeRequest(rawRequest);
        await getComposer().ensureProgramSession(session, request);
        session.requestSnapshot = request;
        session.videoDeviceId = request.primaryCameraDeviceId;
        session.audioDeviceId = request.primaryMicrophoneDeviceId;
        return request;
    }

    function buildRuntimeState(session) {
        const programState = getComposer().getProgramState(session);
        return {
            audioDeviceId: session.audioDeviceId,
            hasMediaStream: Boolean(session.mediaStream),
            liveKit: {
                active: session.liveKitActive,
                connected: session.liveKitConnected,
                publishedAudio: Boolean(session.liveKitPublishedAudioTrack),
                publishedVideo: Boolean(session.liveKitPublishedVideoTrack),
                roomName: session.liveKitRoomName,
                serverUrl: session.liveKitServerUrl
            },
            obs: {
                active: session.obsActive,
                audioAttached: Boolean(session.obsAudioElementId),
                audioElementId: session.obsAudioElementId,
                environment: session.obsEnvironment
            },
            program: {
                audioInputCount: programState.audioInputCount,
                frameRate: programState.frameRate,
                height: session.requestSnapshot?.programVideo?.height ?? 0,
                primarySourceId: programState.primarySourceId,
                videoSourceCount: programState.videoSourceCount,
                width: session.requestSnapshot?.programVideo?.width ?? 0
            },
            recording: {
                active: session.recordingActive,
                fileName: session.recordingFileName,
                mimeType: session.recordingMimeType,
                requestedAudioCodec: session.recordingRequestedAudioCodec,
                requestedContainer: session.recordingRequestedContainer,
                requestedVideoCodec: session.recordingRequestedVideoCodec,
                saveMode: session.recordingSaveMode
            },
            videoDeviceId: session.videoDeviceId
        };
    }

    window[interopNamespace] = {
        async startLiveKitSession(sessionId, rawRequest) {
            const session = ensureSession(sessionId);
            const request = await ensureProgramSession(session, rawRequest);
            const liveKitClient = getLiveKitClient();

            if (!session.liveKitRoom
                || !session.liveKitConnected
                || session.liveKitServerUrl !== request.liveKitServerUrl
                || session.liveKitRoomName !== request.liveKitRoomName) {
                session.liveKitRoom?.disconnect?.();
                session.liveKitRoom = new liveKitClient.Room();
                await session.liveKitRoom.connect(request.liveKitServerUrl, request.liveKitToken);
                session.liveKitConnected = true;
                session.liveKitServerUrl = request.liveKitServerUrl;
                session.liveKitRoomName = request.liveKitRoomName;
            }

            await republishLiveKitTracks(session, liveKitClient);
            session.liveKitActive = true;
        },

        async startLocalRecording(sessionId, rawRequest) {
            const session = ensureSession(sessionId);
            const support = getSupport();
            const request = await ensureProgramSession(session, rawRequest);

            if (session.recordingActive) {
                return;
            }

            session.recordingMimeType = await support.resolveSupportedRecordingMimeType(request);
            session.recordingRequestedContainer = request.recording.containerLabel;
            session.recordingRequestedVideoCodec = request.recording.videoCodecLabel;
            session.recordingRequestedAudioCodec = request.recording.audioCodecLabel;
            await support.prepareRecordingSink(session, request, session.recordingMimeType);
            await support.startRecordingSegment(session);
            session.recordingActive = true;
        },

        async stopLiveKitSession(sessionId) {
            const session = outputSessions.get(sessionId);
            if (!session) {
                return;
            }

            if (session.liveKitPublishedVideoTrack && session.liveKitRoom?.localParticipant) {
                await session.liveKitRoom.localParticipant.unpublishTrack(session.liveKitPublishedVideoTrack, false).catch(() => {});
            }

            if (session.liveKitPublishedAudioTrack && session.liveKitRoom?.localParticipant) {
                await session.liveKitRoom.localParticipant.unpublishTrack(session.liveKitPublishedAudioTrack, false).catch(() => {});
            }

            session.liveKitPublishedVideoTrack = null;
            session.liveKitPublishedAudioTrack = null;
            session.liveKitActive = false;
            session.liveKitConnected = false;
            session.liveKitServerUrl = "";
            session.liveKitRoomName = "";
            session.liveKitRoom?.disconnect?.();
            session.liveKitRoom = null;

            await cleanupSessionIfIdle(sessionId, session);
        },

        async stopLocalRecording(sessionId) {
            const session = outputSessions.get(sessionId);
            if (!session || !session.recordingActive) {
                return;
            }

            await getSupport().stopRecordingSegment(session);
            session.recordingActive = false;
            await getSupport().finalizeRecording(session);

            session.recordingChunks = [];
            session.recordingFileHandle = null;
            session.recordingFileName = "";
            session.recordingMimeType = "";
            session.recordingRequestedAudioCodec = "";
            session.recordingRequestedContainer = "";
            session.recordingRequestedVideoCodec = "";
            session.recordingSaveMode = "";
            session.recordingWritable = null;
            session.recordingWritePromise = Promise.resolve();
            await cleanupSessionIfIdle(sessionId, session);
        },

        async startObsBrowserOutput(sessionId, rawRequest) {
            const session = ensureSession(sessionId);
            await ensureProgramSession(session, rawRequest);
            session.obsActive = true;
            await attachObsAudio(sessionId, session);
        },

        async stopObsBrowserOutput(sessionId) {
            const session = outputSessions.get(sessionId);
            if (!session) {
                return;
            }

            session.obsActive = false;
            await detachObsAudio(session);
            session.obsEnvironment = browserEnvironment;
            await cleanupSessionIfIdle(sessionId, session);
        },

        async updateSessionDevices(sessionId, rawRequest) {
            const session = outputSessions.get(sessionId);
            if (!session) {
                return;
            }

            await ensureProgramSession(session, rawRequest);

            if (session.liveKitActive) {
                await republishLiveKitTracks(session, getLiveKitClient());
            }

            if (session.obsActive) {
                await attachObsAudio(sessionId, session);
            }
        },

        getSessionState(sessionId) {
            const session = outputSessions.get(sessionId);
            return session ? buildRuntimeState(session) : null;
        }
    };
})();
