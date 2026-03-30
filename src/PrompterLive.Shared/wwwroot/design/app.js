// ============================================
// NAVIGATION
// ============================================

const NAVIGATE_EDITOR_METHOD = 'NavigateEditorClient';
const NAVIGATE_HOME_METHOD = 'NavigateHomeClient';
const ROUTE_LIBRARY_PATH = '/library';
const ROUTE_LEARN_PATH = '/learn';
const ROUTE_TELEPROMPTER_PATH = '/teleprompter';
const SCREEN_LIBRARY_ID = 'screen-library';
const SCREEN_RSVP_ID = 'screen-rsvp';
const SCREEN_TELEPROMPTER_ID = 'screen-teleprompter';
const ACTIVE_CLASS_NAME = 'active';
const READER_FOCUS_BUTTON_ID = 'rd-focus-btn';
const READER_GRADIENT_ID = 'rd-gradient';
const READER_GRADIENT_CLASS_NAME = 'rd-gradient';
const READER_HEADER_SEGMENT_ID = 'rd-header-segment';
const READER_PLAY_BUTTON_ID = 'tp-play-btn';
const READER_PROGRESS_FILL_ID = 'rd-progress-fill';
const READER_TIME_SELECTOR = '.rd-time';
const READER_CAMERA_SELECTOR = '.rd-camera[data-camera-role]';
const READER_ACTIVE_WORD_SELECTOR = '.rd-card-active .rd-w.rd-now';
const READER_EDGE_SECTION_SELECTOR = '.rd-edge-section';
const READER_EDGE_SEGMENT_SELECTOR = '.rd-edge-segs > div';
const INTERACTIVE_INPUT_SELECTOR = 'input, [contenteditable]';
const RSVP_PLAY_BUTTON_ID = 'rsvp-play-btn';
const RSVP_SPEED_ID = 'rsvp-speed';
const RSVP_WPM_BADGE_ID = 'rsvp-wpm-badge';
const DEFAULT_SEGMENT_NAME = 'Intro';
const DEFAULT_SEGMENT_EMOTION = 'warm';
const DEFAULT_SEGMENT_BACKGROUND = 'warm';
const KEY_ESCAPE = 'Escape';
const KEY_SPACE = ' ';
const KEY_ARROW_RIGHT = 'ArrowRight';
const KEY_ARROW_LEFT = 'ArrowLeft';
const KEY_PAGE_DOWN = 'PageDown';
const KEY_PAGE_UP = 'PageUp';
const KEY_ARROW_UP = 'ArrowUp';
const KEY_ARROW_DOWN = 'ArrowDown';
const KEY_CAMERA_LOWER = 'c';
const KEY_CAMERA_UPPER = 'C';
const KEY_FOCUS_LOWER = 'f';
const KEY_FOCUS_UPPER = 'F';
let blazorNavigator = null;

function requestEditorNavigation() {
    if (blazorNavigator) {
        void blazorNavigator.invokeMethodAsync(NAVIGATE_EDITOR_METHOD);
    }
}

function requestHomeNavigation() {
    if (blazorNavigator) {
        void blazorNavigator.invokeMethodAsync(NAVIGATE_HOME_METHOD);
    }
}

function getCurrentPathName() {
    return (window.location.pathname || '').toLowerCase();
}

function isCurrentPath(path) {
    return getCurrentPathName() === path;
}

function isTeleprompterRuntimeActive() {
    return isCurrentPath(ROUTE_TELEPROMPTER_PATH) && document.getElementById(SCREEN_TELEPROMPTER_ID) !== null;
}

function isRsvpRuntimeActive() {
    return isCurrentPath(ROUTE_LEARN_PATH) && document.getElementById(SCREEN_RSVP_ID) !== null;
}

function isLibraryRuntimeActive() {
    return isCurrentPath(ROUTE_LIBRARY_PATH) && document.getElementById(SCREEN_LIBRARY_ID) !== null;
}

// ============================================
// RSVP (Learn mode — simple word-by-word)
// ============================================

const RSVP_CONTEXT_WORD_COUNT = 5;
const RSVP_END_OF_SCRIPT = 'End of script.';
const RSVP_MAX_SPEED = 600;
const RSVP_MIN_SPEED = 100;
const RSVP_MIN_WORD_DURATION_MS = 120;
const RSVP_NEUTRAL_EMOTION = 'neutral';

let rsvpSpeed = 300;
let rsvpPlaying = false;
let rsvpTimeline = [];
let rsvpTimer = null;

function changeRsvpSpeed(delta) {
    rsvpSpeed = Math.max(RSVP_MIN_SPEED, Math.min(RSVP_MAX_SPEED, rsvpSpeed + delta));
    const el = document.getElementById(RSVP_SPEED_ID);
    if (el) el.textContent = rsvpSpeed;
    const badge = document.getElementById(RSVP_WPM_BADGE_ID);
    if (badge) badge.textContent = rsvpSpeed + ' WPM';

    updateRsvpProgress();
    if (rsvpPlaying) {
        scheduleRsvpAdvance();
    }
}

function toggleRsvp() {
    rsvpPlaying = !rsvpPlaying;
    const btn = document.getElementById(RSVP_PLAY_BUTTON_ID);
    if (!btn) {
        return;
    }

    if (rsvpPlaying) {
        btn.innerHTML = '<svg width="28" height="28" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2"><rect x="6" y="4" width="4" height="16"/><rect x="14" y="4" width="4" height="16"/></svg>';
        scheduleRsvpAdvance();
    } else {
        btn.innerHTML = '<svg width="28" height="28" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2"><polygon points="5,3 19,12 5,21"/></svg>';
        clearTimeout(rsvpTimer);
    }
}

let rsvpWords = [
    'Good', 'morning', 'everyone,', 'and', 'welcome', 'to', 'what', 'I', 'believe',
    'will', 'be', 'a', 'transformative', 'moment', 'for', 'our', 'company.',
    'Today,', "we're", 'not', 'just', 'launching', 'a', 'product', '—',
    "we're", 'introducing', 'a', 'solution', 'that', 'will', 'revolutionize',
    'how', 'our', 'customers', 'interact', 'with', 'technology.'
];

let rsvpIndex = 0;

function getRsvpEntries() {
    if (Array.isArray(rsvpTimeline) && rsvpTimeline.length > 0) {
        return rsvpTimeline;
    }

    return rsvpWords.map(word => ({
        word,
        durationMs: Math.round(60000 / Math.max(rsvpSpeed, 1)),
        pauseAfterMs: 0,
        baseWpm: rsvpSpeed,
        nextPhrase: '',
        emotion: RSVP_NEUTRAL_EMOTION
    }));
}

function getORP(word) {
    const len = word.replace(/[^a-zA-Z]/g, '').length;
    if (len <= 1) return 0;
    if (len <= 5) return 1;
    if (len <= 9) return 2;
    return 3;
}

function renderRsvpWord() {
    const entries = getRsvpEntries();
    if (!entries.length) {
        return;
    }

    const safeIndex = Math.max(0, Math.min(rsvpIndex, entries.length - 1));
    rsvpIndex = safeIndex;
    const entry = entries[safeIndex];
    const word = entry.word;
    const orp = getORP(word);
    const container = document.getElementById('rsvp-word');
    if (!container) return;

    // Render focus word with ORP
    let html = '';
    for (let i = 0; i < word.length; i++) {
        html += `<span${i === orp ? ' class="orp"' : ''}>${word[i]}</span>`;
    }
    container.innerHTML = html;
    container.style.transform = '';

    // Position the word so the ORP letter aligns with the center vertical line
    const orpEl = container.querySelector('.orp');
    if (orpEl) {
        const containerRect = container.parentElement.getBoundingClientRect();
        const orpRect = orpEl.getBoundingClientRect();
        const containerCenter = containerRect.left + containerRect.width / 2;
        const orpCenter = orpRect.left + orpRect.width / 2;
        const shift = containerCenter - orpCenter;
        container.style.transform = `translateX(${shift}px)`;
    }

    // Left context (5 words)
    const leftEl = document.getElementById('rsvp-ctx-l');
    if (leftEl) {
        const leftWords = entries
            .slice(Math.max(0, safeIndex - RSVP_CONTEXT_WORD_COUNT), safeIndex)
            .map(item => item.word);
        leftEl.innerHTML = leftWords.map(w => `<span>${w}</span>`).join('');
    }

    // Right context (5 words)
    const rightEl = document.getElementById('rsvp-ctx-r');
    if (rightEl) {
        const rightWords = entries
            .slice(safeIndex + 1, safeIndex + RSVP_CONTEXT_WORD_COUNT + 1)
            .map(item => item.word);
        rightEl.innerHTML = rightWords.map(w => `<span>${w}</span>`).join('');
    }

    const fill = document.querySelector('.rsvp-progress-fill');
    if (fill) fill.style.width = ((safeIndex + 1) / entries.length * 100) + '%';

    const phrase = document.getElementById('rsvp-next-phrase');
    if (phrase) {
        phrase.textContent = entry.nextPhrase || entries
            .slice(safeIndex + 1, Math.min(safeIndex + 10, entries.length))
            .map(item => item.word)
            .join(' ') || RSVP_END_OF_SCRIPT;
    }

    updateRsvpProgress();
    animateRsvpPause(entry);
}

function getScaledRsvpDuration(entry, key) {
    const sourceValue = Number(entry?.[key]);
    const baseValue = Number(entry?.baseWpm) || rsvpSpeed;
    const scaled = (Number.isFinite(sourceValue) ? sourceValue : Math.round(60000 / Math.max(baseValue, 1)))
        * (baseValue / Math.max(rsvpSpeed, 1));
    return Math.max(key === 'pauseAfterMs' ? 0 : RSVP_MIN_WORD_DURATION_MS, Math.round(scaled));
}

function updateRsvpProgress() {
    const entries = getRsvpEntries();
    const progress = document.getElementById('rsvp-progress-label');
    if (progress) {
        const remainingMilliseconds = entries
            .slice(Math.max(0, rsvpIndex + 1))
            .reduce((sum, entry) => sum + getScaledRsvpDuration(entry, 'durationMs') + getScaledRsvpDuration(entry, 'pauseAfterMs'), 0);
        const remainingSeconds = Math.ceil(remainingMilliseconds / 1000);
        progress.textContent = `Word ${Math.min(rsvpIndex + 1, entries.length)} / ${entries.length} · ~${Math.floor(remainingSeconds / 60)}:${String(remainingSeconds % 60).padStart(2, '0')} left`;
    }
}

function animateRsvpPause(entry) {
    const pauseFill = document.querySelector('.rsvp-pause-fill');
    if (!pauseFill) {
        return;
    }

    const pauseMs = getScaledRsvpDuration(entry, 'pauseAfterMs');
    pauseFill.style.transition = 'none';
    pauseFill.style.width = '0%';
    void pauseFill.offsetWidth;

    if (pauseMs <= 0) {
        return;
    }

    pauseFill.style.transition = `width ${pauseMs}ms linear`;
    pauseFill.style.width = '100%';
}

function scheduleRsvpAdvance() {
    clearTimeout(rsvpTimer);

    if (!rsvpPlaying) {
        return;
    }

    const entries = getRsvpEntries();
    if (!entries.length) {
        return;
    }

    const current = entries[rsvpIndex] || entries[0];
    const delay = getScaledRsvpDuration(current, 'durationMs') + getScaledRsvpDuration(current, 'pauseAfterMs');
    rsvpTimer = setTimeout(() => {
        if (!rsvpPlaying) {
            return;
        }

        rsvpIndex = (rsvpIndex + 1) % entries.length;
        renderRsvpWord();
        scheduleRsvpAdvance();
    }, Math.max(150, delay));
}

// Step RSVP by N words (negative = back)
function stepRsvpWord(n) {
    const entries = getRsvpEntries();
    if (!entries.length) {
        return;
    }

    rsvpIndex = (rsvpIndex + n + entries.length) % entries.length;
    renderRsvpWord();
    if (rsvpPlaying) {
        scheduleRsvpAdvance();
    }
}

// ============================================
// READER — card-by-card with word highlighting
// ============================================

let tpSpeed = 140;
let tpPlaying = false; // Start paused — user presses play
let readerWordIndex = -1;
let readerCardIndex = 0;
let countdownActive = false;
const READER_DEFAULT_WORD_DELAY_MS = 600;
const READER_MIN_WORD_DELAY_MS = 120;
const READER_CARD_TRANSITION_DELAY_MS = 850;

function getReaderWordDuration(word) {
    const duration = Number.parseInt(word?.dataset?.ms || '600', 10);
    return Number.isFinite(duration) && duration > 0 ? duration : 600;
}

function getReaderWordPause(word) {
    const pause = Number.parseInt(word?.dataset?.pauseMs || '0', 10);
    return Number.isFinite(pause) && pause > 0 ? pause : 0;
}

function getReaderTotalMilliseconds() {
    const timeEl = document.querySelector('.rd-time');
    const parsed = Number.parseInt(timeEl?.dataset?.totalMs || '147000', 10);
    return Number.isFinite(parsed) && parsed > 0 ? parsed : 147000;
}

function changeTpSpeed(delta) {
    tpSpeed = Math.max(80, Math.min(220, tpSpeed + delta));
    const el = document.getElementById('tp-speed');
    if (el) el.textContent = tpSpeed;
}

function toggleTp() {
    if (countdownActive) return; // Ignore during countdown

    if (tpPlaying) {
        // Pause — keep current word highlighted, just stop advancing
        tpPlaying = false;
        clearTimeout(readerTimer);
        const btn = document.getElementById('tp-play-btn');
        if (btn) btn.innerHTML = '<svg width="22" height="22" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2"><polygon points="5,3 19,12 5,21"/></svg>';
    } else if (readerWordIndex > 0) {
        // Resume from pause — no countdown, just continue
        startPlaying();
    } else {
        // First start — full countdown 3-2-1
        startCountdown();
    }
}

function startCountdown() {
    countdownActive = true;
    const overlay = document.getElementById('rd-countdown');
    if (!overlay) return startPlaying();

    // Show overlay with dark backdrop first (pre-pause)
    overlay.textContent = '';
    overlay.classList.add('active');
    overlay.classList.remove('rd-pulse');

    // Flow: 3 → 2 → 1 → overlay gone → empty beat (no highlight) → first word lights up
    setTimeout(() => {
        let count = 3;
        overlay.textContent = count;

        const tick = setInterval(() => {
            count--;
            if (count > 0) {
                overlay.textContent = count;
            } else {
                // "1" shown → clear overlay instantly
                clearInterval(tick);
                overlay.textContent = '';
                overlay.classList.remove('active');
                countdownActive = false;
                // Empty beat — text visible but NO word highlighted
                setTimeout(() => {
                    // First word lights up → startPlaying schedules
                    // the next word after a full 600ms interval
                    highlightFirstWord();
                    startPlaying();
                }, 700);
            }
        }, 700);
    }, 600);
}

// Light up the first word — visual cue that reading begins.
// Don't re-center: preCenterCard already positioned text for word 0.
function highlightFirstWord() {
    const cards = getReaderCards();
    const card = cards[readerCardIndex];
    if (!card) return;
    readerWordIndex = 0;
    setActiveReaderWord(card, readerWordIndex);
    updateReaderProgress();
}

function startPlaying() {
    tpPlaying = true;
    countdownActive = false;
    const btn = document.getElementById(READER_PLAY_BUTTON_ID);
    if (btn) btn.innerHTML = '<svg width="22" height="22" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2"><rect x="6" y="4" width="4" height="16"/><rect x="14" y="4" width="4" height="16"/></svg>';
    scheduleNextWord();
}

function getReaderCards() {
    return document.querySelectorAll('.rd-cluster-wrap .rd-card');
}

function showCard(index) {
    const cards = getReaderCards();
    if (!cards.length) return;

    cards.forEach((card, i) => {
        card.classList.remove('rd-card-active', 'rd-card-prev', 'rd-card-next');
        if (i === index) {
            card.classList.add('rd-card-active');
        } else if (i === index - 1) {
            card.classList.add('rd-card-prev');
        } else if (i === index + 1) {
            card.classList.add('rd-card-next');
        } else if (i < index) {
            card.classList.add('rd-card-prev');
        } else {
            card.classList.add('rd-card-next');
        }
    });

    // Update segment info and block indicator
    goToSegment(index);
    updateBlockIndicator();
}

function formatSeconds(totalSeconds) {
    const safeSeconds = Math.max(0, totalSeconds | 0);
    const minutes = Math.floor(safeSeconds / 60);
    const seconds = String(safeSeconds % 60).padStart(2, '0');
    return `${minutes}:${seconds}`;
}

function getReaderTotalSeconds() {
    return Math.max(1, Math.ceil(getReaderTotalMilliseconds() / 1000));
}

function getReaderElapsedMilliseconds() {
    const cards = getReaderCards();
    let elapsed = 0;

    cards.forEach((card, cardIndex) => {
        const words = Array.from(card.querySelectorAll('.rd-w'));
        if (cardIndex < readerCardIndex) {
            words.forEach(word => {
                elapsed += getReaderWordDuration(word) + getReaderWordPause(word);
            });
            return;
        }

        if (cardIndex !== readerCardIndex || readerWordIndex <= 0) {
            return;
        }

        words.slice(0, readerWordIndex).forEach(word => {
            elapsed += getReaderWordDuration(word) + getReaderWordPause(word);
        });
    });

    return elapsed;
}

function updateReaderProgress() {
    const totalMilliseconds = getReaderTotalMilliseconds();
    const elapsedMilliseconds = Math.min(totalMilliseconds, getReaderElapsedMilliseconds());
    const progress = totalMilliseconds > 0 ? (elapsedMilliseconds / totalMilliseconds) * 100 : 0;
    const fill = document.getElementById('rd-progress-fill');
    if (fill) {
        fill.style.width = progress + '%';
    }

    const timeEl = document.querySelector('.rd-time');
    if (timeEl) {
        timeEl.textContent = `${formatSeconds(Math.floor(elapsedMilliseconds / 1000))} / ${formatSeconds(Math.ceil(totalMilliseconds / 1000))}`;
    }
}

// Center the active word's line at screen center
// Center the active word at the focal point (30% from top)
// smooth=true uses CSS transition, smooth=false is instant (no flash)
function centerActiveLine(smooth) {
    const word = document.querySelector('.rd-card-active .rd-w.rd-now');
    if (!word) return;
    const text = word.closest('.rd-cluster-text');
    if (!text) return;
    const stage = document.querySelector('.rd-stage');
    if (!stage) return;

    // Disable transition to measure natural position without flash
    text.style.transition = 'none';
    const prevTy = text.style.transform;
    text.style.transform = 'none';
    void text.offsetHeight;

    const stageRect = stage.getBoundingClientRect();
    const wordRect = word.getBoundingClientRect();
    const focalPoint = stageRect.top + stageRect.height * (rdFocalPct / 100);
    const wordCenter = wordRect.top + wordRect.height / 2;
    const ty = focalPoint - wordCenter;

    if (smooth) {
        // Restore previous position instantly, then animate to new
        text.style.transform = prevTy || 'none';
        void text.offsetHeight;
        text.style.transition = '';
        text.style.transform = `translateY(${ty}px)`;
    } else {
        // Set instantly (for new card entry — no jump)
        text.style.transform = `translateY(${ty}px)`;
        void text.offsetHeight;
        requestAnimationFrame(() => { text.style.transition = ''; });
    }
}

// Pre-position text on a card BEFORE it becomes visible
// so it slides in already centered — no jump after transition
function preCenterCard(card) {
    if (!card) return;
    const firstWord = card.querySelector('.rd-w');
    if (!firstWord) return;
    const text = card.querySelector('.rd-cluster-text');
    if (!text) return;
    const stage = document.querySelector('.rd-stage');
    if (!stage) return;

    // Temporarily make card visible but transparent to measure
    const prevOpacity = card.style.opacity;
    const prevTransform = card.style.transform;
    card.style.opacity = '0';
    card.style.transition = 'none';
    card.style.transform = 'translateY(0)';
    text.style.transition = 'none';
    text.style.transform = 'none';
    firstWord.classList.add('rd-now');
    void text.offsetHeight;

    const stageRect = stage.getBoundingClientRect();
    const wordRect = firstWord.getBoundingClientRect();
    const focalPoint = stageRect.top + stageRect.height * (rdFocalPct / 100);
    const ty = focalPoint - (wordRect.top + wordRect.height / 2);

    // Set text position and restore card to off-screen
    text.style.transform = `translateY(${ty}px)`;
    card.style.transform = prevTransform;
    card.style.opacity = prevOpacity;
    void card.offsetHeight;
    card.style.transition = '';
    text.style.transition = '';
}

function setActiveReaderWord(card, activeIndex) {
    if (!card) return null;

    const words = card.querySelectorAll('.rd-w');
    if (!words.length || activeIndex < 0 || activeIndex >= words.length) {
        return null;
    }

    words.forEach((word, index) => {
        word.classList.remove('rd-now');
        if (index < activeIndex) {
            word.classList.add('rd-read');
            return;
        }

        word.classList.remove('rd-read');
        if (index === activeIndex) {
            word.classList.add('rd-now');
        }
    });

    card.querySelectorAll('.rd-g').forEach(group => group.classList.remove('rd-g-active'));
    const currentWord = words[activeIndex];
    const currentGroup = currentWord.closest('.rd-g');
    if (currentGroup) {
        currentGroup.classList.add('rd-g-active');
    }

    return currentWord;
}

function pauseReaderPlaybackForTransition() {
    const shouldResume = tpPlaying;
    tpPlaying = false;
    clearTimeout(readerTimer);
    return shouldResume;
}

function resumeReaderPlaybackAfterTransition(shouldResume) {
    if (!shouldResume) {
        return;
    }

    tpPlaying = true;
    scheduleNextWord();
}

function advanceReaderWord() {
    const cards = getReaderCards();
    if (!cards.length) return;

    const activeCard = cards[readerCardIndex];
    if (!activeCard) return;

    const words = activeCard.querySelectorAll('.rd-w');
    if (!words.length) return;

    readerWordIndex++;

    // If we've gone past all words in this card, move to next card
    if (readerWordIndex >= words.length) {
        readerWordIndex = -1;
        readerCardIndex++;
        if (readerCardIndex >= cards.length) {
            readerCardIndex = 0;
            cards.forEach(c => {
                c.querySelectorAll('.rd-w').forEach(w => w.classList.remove('rd-read', 'rd-now'));
                c.querySelectorAll('.rd-g').forEach(g => g.classList.remove('rd-g-active'));
                const t = c.querySelector('.rd-cluster-text');
                if (t) { t.style.transform = ''; t.dataset.ty = '0'; }
            });
        }
        // Pre-center next card BEFORE it slides in — no jump
        const nextCard = cards[readerCardIndex];
        if (nextCard) preCenterCard(nextCard);

        const shouldResume = pauseReaderPlaybackForTransition();
        showCard(readerCardIndex);

        // Wait for slide transition, then resume
        setTimeout(() => {
            readerWordIndex = 0;
            setActiveReaderWord(cards[readerCardIndex], readerWordIndex);
            updateReaderProgress();
            resumeReaderPlaybackAfterTransition(shouldResume);
        }, READER_CARD_TRANSITION_DELAY_MS);
        return;
    }

    setActiveReaderWord(activeCard, readerWordIndex);
    // Center the active line on screen (smooth scroll)
    centerActiveLine(true);
    updateReaderProgress();
}

// Font size control
let rdFontSize = 36;
function changeFontSize(delta) {
    rdFontSize = Math.max(24, Math.min(56, rdFontSize + delta));
    const wrap = document.querySelector('.rd-cluster-wrap');
    if (wrap) wrap.style.setProperty('--rd-font-size', rdFontSize + 'px');
    const label = document.getElementById('rd-font-label');
    if (label) label.textContent = rdFontSize;
}

// Focal point control (vertical position of reading line)
let rdFocalPct = 30;
let focalGuideTimer = null;
function changeFocalPoint(val) {
    rdFocalPct = parseInt(val);
    const label = document.getElementById('rd-focal-val');
    if (label) label.textContent = rdFocalPct + '%';
    // Show horizontal guide line at the focal point
    const guide = document.getElementById('rd-guide-h');
    if (guide) {
        guide.style.top = rdFocalPct + '%';
        guide.classList.add('active');
        clearTimeout(focalGuideTimer);
        focalGuideTimer = setTimeout(() => guide.classList.remove('active'), 800);
    }
    // Re-center the active line at the new focal point
    centerActiveLine(true);
}

// Text width control
let rdTextWidth = 750;
let widthGuideTimer = null;
function changeTextWidth(val) {
    rdTextWidth = parseInt(val);
    const wrap = document.querySelector('.rd-cluster-wrap');
    if (wrap) wrap.style.maxWidth = rdTextWidth + 'px';
    const label = document.getElementById('rd-width-val');
    if (label) label.textContent = rdTextWidth;
    // Show vertical guide lines at text edges
    const stage = document.querySelector('.rd-stage');
    const guideL = document.getElementById('rd-guide-v-l');
    const guideR = document.getElementById('rd-guide-v-r');
    if (stage && wrap && guideL && guideR) {
        const stageRect = stage.getBoundingClientRect();
        const wrapRect = wrap.getBoundingClientRect();
        guideL.style.left = (wrapRect.left - stageRect.left) + 'px';
        guideR.style.left = (wrapRect.right - stageRect.left) + 'px';
        guideL.classList.add('active');
        guideR.classList.add('active');
        clearTimeout(widthGuideTimer);
        widthGuideTimer = setTimeout(() => {
            guideL.classList.remove('active');
            guideR.classList.remove('active');
        }, 800);
    }
    // Re-center after width change (line positions may shift)
    centerActiveLine(true);
}

// Step forward/back by one word (manual word navigation)
function stepWord(direction) {
    const cards = getReaderCards();
    if (!cards.length) return;
    const activeCard = cards[readerCardIndex];
    if (!activeCard) return;
    const words = activeCard.querySelectorAll('.rd-w');
    if (!words.length) return;

    if (direction > 0) {
        // Forward — same as advanceReaderWord but manual
        advanceReaderWord();
    } else {
        // Back — go to previous word
        if (readerWordIndex > 0) {
            // Un-read current word
            const cur = words[readerWordIndex];
            if (cur) cur.classList.remove('rd-now');
            readerWordIndex--;
            // Update states
            words.forEach((w, i) => {
                w.classList.remove('rd-now', 'rd-read');
                if (i < readerWordIndex) w.classList.add('rd-read');
                else if (i === readerWordIndex) w.classList.add('rd-now');
            });
            // Update phrase group
            activeCard.querySelectorAll('.rd-g').forEach(g => g.classList.remove('rd-g-active'));
            const cw = words[readerWordIndex];
            if (cw) {
                const pg = cw.closest('.rd-g');
                if (pg) pg.classList.add('rd-g-active');
            }
            centerActiveLine(true);
            updateReaderProgress();
        }
    }
}

// Jump to next/prev card
function jumpCard(direction) {
    const cards = getReaderCards();

    // ── Back button: first press → start of current block,
    //    second press (if already at start) → previous block.
    //    Like a music player: tap back = restart song, tap again = prev song.
    if (direction < 0) {
        if (readerWordIndex > 1) {
            // Not at start yet — rewind to beginning of current block
            const card = cards[readerCardIndex];
            if (card) {
                card.querySelectorAll('.rd-w').forEach(w => w.classList.remove('rd-read', 'rd-now'));
                card.querySelectorAll('.rd-g').forEach(g => g.classList.remove('rd-g-active'));
                const t = card.querySelector('.rd-cluster-text');
                if (t) { t.style.transform = ''; t.dataset.ty = '0'; }
                preCenterCard(card);
                // Remove rd-now from preCenterCard measurement
                card.querySelectorAll('.rd-w').forEach(w => w.classList.remove('rd-now'));
                card.querySelectorAll('.rd-g').forEach(g => g.classList.remove('rd-g-active'));
            }
            readerWordIndex = 0;
            highlightFirstWord();
            return;
        }
        // Already at start — fall through to jump to previous block
    }

    const newIndex = readerCardIndex + direction;
    if (newIndex < 0 || newIndex >= cards.length) return;

    // Reset words and text position in current card
    const currentCard = cards[readerCardIndex];
    if (currentCard) {
        currentCard.querySelectorAll('.rd-w').forEach(w => w.classList.remove('rd-read', 'rd-now'));
        currentCard.querySelectorAll('.rd-g').forEach(g => g.classList.remove('rd-g-active'));
        const t = currentCard.querySelector('.rd-cluster-text');
        if (t) { t.style.transform = ''; t.dataset.ty = '0'; }
    }
    // Pre-center target card before it slides in
    const targetCard = cards[newIndex];
    if (targetCard) {
        targetCard.querySelectorAll('.rd-w').forEach(w => w.classList.remove('rd-read', 'rd-now'));
        targetCard.querySelectorAll('.rd-g').forEach(g => g.classList.remove('rd-g-active'));
        const t = targetCard.querySelector('.rd-cluster-text');
        if (t) { t.style.transform = ''; t.dataset.ty = '0'; }
        preCenterCard(targetCard);
        // Remove rd-now from preCenterCard measurement
        targetCard.querySelectorAll('.rd-w').forEach(w => w.classList.remove('rd-now'));
    }

    const shouldResume = pauseReaderPlaybackForTransition();
    readerCardIndex = newIndex;
    readerWordIndex = 0;
    showCard(readerCardIndex);
    updateReaderProgress();

    // Wait for slide transition, then resume
    setTimeout(() => {
        setActiveReaderWord(cards[readerCardIndex], readerWordIndex);
        updateReaderProgress();
        resumeReaderPlaybackAfterTransition(shouldResume);
    }, READER_CARD_TRANSITION_DELAY_MS);
}

// Update block indicator
function updateBlockIndicator() {
    const el = document.getElementById('rd-block-indicator');
    const cards = getReaderCards();
    if (el) el.textContent = (readerCardIndex + 1) + ' / ' + cards.length;
}

function resetReader() {
    readerWordIndex = -1;
    readerCardIndex = 0;
    tpPlaying = false; // Start paused
    countdownActive = false;
    clearTimeout(readerTimer);
    const cards = getReaderCards();
    cards.forEach(c => {
        c.querySelectorAll('.rd-w').forEach(w => w.classList.remove('rd-read', 'rd-now'));
        c.querySelectorAll('.rd-g').forEach(g => g.classList.remove('rd-g-active'));
        const t = c.querySelector('.rd-cluster-text');
        if (t) { t.style.transform = ''; t.dataset.ty = '0'; }
    });
    // Pre-center first card, then show it — no jump
    const firstCard = cards[0];
    if (firstCard) preCenterCard(firstCard);
    // Remove rd-now that preCenterCard added for measurement —
    // no word should be bright until countdown finishes
    cards.forEach(c => {
        c.querySelectorAll('.rd-w').forEach(w => w.classList.remove('rd-now'));
        c.querySelectorAll('.rd-g').forEach(g => g.classList.remove('rd-g-active'));
    });
    showCard(0);

    // Show play button (paused state)
    const btn = document.getElementById('tp-play-btn');
    if (btn) btn.innerHTML = '<svg width="22" height="22" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2"><polygon points="5,3 19,12 5,21"/></svg>';
    // Reset progress
    const fill = document.getElementById(READER_PROGRESS_FILL_ID);
    if (fill) fill.style.width = '0%';
    const timeEl = document.querySelector(READER_TIME_SELECTOR);
    if (timeEl) {
        timeEl.textContent = `0:00 / ${formatSeconds(getReaderTotalSeconds())}`;
    }
}

// Auto-advance reader — timeout chain guarantees each word gets full duration.
// setInterval would fire on a fixed clock, cutting words short on resume.
let readerTimer = null;
let readerCameraRequestedState = null;
let readerCameraOperationId = 0;
function scheduleNextWord() {
    clearTimeout(readerTimer);
    if (!tpPlaying) {
        return;
    }

    const currentWord = document.querySelector(READER_ACTIVE_WORD_SELECTOR);
    const delay = currentWord
        ? getReaderWordDuration(currentWord) + getReaderWordPause(currentWord)
        : READER_DEFAULT_WORD_DELAY_MS;

    readerTimer = setTimeout(() => {
        if (tpPlaying && isTeleprompterRuntimeActive()) {
            advanceReaderWord();
        }
        if (tpPlaying) scheduleNextWord();
    }, Math.max(READER_MIN_WORD_DELAY_MS, delay));
}

function getReaderCameraElements() {
    return Array.from(document.querySelectorAll(READER_CAMERA_SELECTOR));
}

function applyReaderCameraVisualState(cameras, tint, btn, nextActive) {
    cameras.forEach(camera => camera.classList.toggle('active', nextActive));
    tint.classList.toggle('active', nextActive);
    btn.classList.toggle('active', nextActive);
}

// Camera toggle
async function setReaderCameraActive(nextActive) {
    const cameras = getReaderCameraElements();
    const tint = document.getElementById('rd-camera-tint');
    const btn = document.getElementById('rd-cam-btn');
    if (!cameras.length || !tint || !btn) {
        return;
    }

    readerCameraRequestedState = !!nextActive;
    const operationId = ++readerCameraOperationId;
    const attachableCameras = cameras.filter(camera => (camera.dataset.cameraDeviceId || '').length > 0);
    const targetCameras = attachableCameras.length > 0
        ? attachableCameras
        : cameras.slice(0, 1);

    applyReaderCameraVisualState(cameras, tint, btn, nextActive);

    if (nextActive) {
        await Promise.all(targetCameras.map(async camera => {
            try {
                await window.PrompterLive.media.attachCamera(camera.id, camera.dataset.cameraDeviceId || '', true);
            } catch {
                // Leave the visual state in place even if the browser blocks access.
            }
        }));
    } else {
        await Promise.all(cameras.map(async camera => {
            try {
                await window.PrompterLive.media.detachCamera(camera.id);
            } catch {
            }
        }));
    }

    if (operationId !== readerCameraOperationId) {
        return;
    }

    applyReaderCameraVisualState(cameras, tint, btn, nextActive);
}

async function toggleReaderCamera() {
    const cameras = getReaderCameraElements();
    if (!cameras.length) {
        return;
    }

    const isActive = cameras.some(camera => camera.classList.contains('active'));
    await setReaderCameraActive(!isActive);
}

async function initializeReaderCamera() {
    const cameras = getReaderCameraElements();
    if (!cameras.length) {
        return;
    }

    const shouldAutoStart = cameras.some(camera => camera.dataset.cameraAutostart === 'true');
    const requestedState = readerCameraRequestedState ?? shouldAutoStart;
    await setReaderCameraActive(requestedState);
}

// Focus mode toggle
function toggleFocusMode() {
    const btn = document.getElementById(READER_FOCUS_BUTTON_ID);
    if (!btn) {
        return;
    }

    btn.classList.toggle(ACTIVE_CLASS_NAME);
    // Could hide header/footer for immersive mode
}

// ============================================
// KEYBOARD SHORTCUTS
// ============================================

let currentSegmentIndex = 0;

function getReaderSegmentMeta(index) {
    const cards = getReaderCards();
    const card = cards[index];
    if (!card) {
        return {
            name: DEFAULT_SEGMENT_NAME,
            emotion: DEFAULT_SEGMENT_EMOTION,
            bg: DEFAULT_SEGMENT_BACKGROUND
        };
    }

    return {
        name: card.dataset.segmentName || `Block ${index + 1}`,
        emotion: card.dataset.segmentEmotion || 'neutral',
        bg: card.dataset.segmentBg || 'neutral'
    };
}

function goToSegment(index) {
    const cards = getReaderCards();
    if (!cards.length) return;
    if (index < 0) index = 0;
    if (index >= cards.length) index = cards.length - 1;
    currentSegmentIndex = index;

    const seg = getReaderSegmentMeta(index);

    // Update gradient background with smooth transition
    const gradient = document.getElementById(READER_GRADIENT_ID);
    if (gradient) {
        gradient.className = READER_GRADIENT_CLASS_NAME;
        if (seg.bg) gradient.classList.add(seg.bg);
    }

    // Update edge section label
    const sectionEl = document.querySelector(READER_EDGE_SECTION_SELECTOR);
    if (sectionEl) sectionEl.textContent = seg.name;

    // Update header subtitle
    const headerSeg = document.getElementById(READER_HEADER_SEGMENT_ID);
    if (headerSeg) headerSeg.textContent = seg.name + ' · ' + seg.emotion.charAt(0).toUpperCase() + seg.emotion.slice(1);

    // Update segment indicator colors
    const segs = document.querySelectorAll(READER_EDGE_SEGMENT_SELECTOR);
    segs.forEach((s, i) => {
        s.style.opacity = i === index ? '1' : '0.4';
    });
}

function nextSegment() { goToSegment(currentSegmentIndex + 1); }
function prevSegment() { goToSegment(currentSegmentIndex - 1); }

// ── Keyboard shortcuts ──
document.addEventListener('keydown', (e) => {
    const isTp = isTeleprompterRuntimeActive();
    const isRsvp = isRsvpRuntimeActive();

    if (e.key === KEY_ESCAPE) {
        if (!isLibraryRuntimeActive()) {
            if (isTp || isRsvp) {
                requestEditorNavigation();
            } else {
                requestHomeNavigation();
            }
        }
        return;
    }

    // Only process shortcuts when in teleprompter or RSVP mode
    if (!isTp && !isRsvp) return;
    if (e.target.matches(INTERACTIVE_INPUT_SELECTOR)) return;

    switch (e.key) {
        // ── Play / Pause ──
        case KEY_SPACE:
            e.preventDefault();
            if (isRsvp) toggleRsvp();
            if (isTp) toggleTp();
            break;

        // ── Segment navigation ──
        case KEY_ARROW_RIGHT:
        case KEY_PAGE_DOWN:
            e.preventDefault();
            if (isTp) nextSegment();
            break;
        case KEY_ARROW_LEFT:
        case KEY_PAGE_UP:
            e.preventDefault();
            if (isTp) prevSegment();
            break;

        // ── Speed ──
        case KEY_ARROW_UP:
            e.preventDefault();
            if (isTp) changeTpSpeed(10);
            if (isRsvp) changeRsvpSpeed(10);
            break;
        case KEY_ARROW_DOWN:
            e.preventDefault();
            if (isTp) changeTpSpeed(-10);
            if (isRsvp) changeRsvpSpeed(-10);
            break;

        // ── Camera toggle ──
        case KEY_CAMERA_LOWER:
        case KEY_CAMERA_UPPER:
            if (isTp) toggleReaderCamera();
            break;

        // ── Focus mode ──
        case KEY_FOCUS_LOWER:
        case KEY_FOCUS_UPPER:
            if (isTp) toggleFocusMode();
            break;
    }
});

// ============================================
// INIT
// ============================================

window.PrompterLiveDesign = {
    attachNavigator(dotNetRef) {
        blazorNavigator = dotNetRef || null;
    },
    initialize(screenId) {
        const resolvedScreenId = screenId || '';

        if (resolvedScreenId === 'rsvp') {
            renderRsvpWord();
        }

        if (resolvedScreenId === 'teleprompter') {
            resetReader();
            void initializeReaderCamera();
        }
    },
    setRsvpTimeline(entries, options) {
        const nextEntries = Array.isArray(entries)
            ? entries
                .filter(entry => entry && typeof entry.word === 'string' && entry.word.trim().length > 0)
                .map(entry => ({
                    word: entry.word,
                    durationMs: Number.isFinite(entry.durationMs) ? entry.durationMs : Math.round(60000 / Math.max(rsvpSpeed, 1)),
                    pauseAfterMs: Number.isFinite(entry.pauseAfterMs) ? entry.pauseAfterMs : 0,
                    baseWpm: Number.isFinite(entry.baseWpm) ? entry.baseWpm : rsvpSpeed,
                    nextPhrase: typeof entry.nextPhrase === 'string' ? entry.nextPhrase : '',
                    emotion: typeof entry.emotion === 'string' ? entry.emotion : RSVP_NEUTRAL_EMOTION
                }))
            : [];

        if (!nextEntries.length) {
            return;
        }

        clearTimeout(rsvpTimer);
        rsvpTimeline = nextEntries;
        rsvpWords = nextEntries.map(entry => entry.word);
        rsvpIndex = Math.max(0, Math.min(options?.index ?? 0, rsvpTimeline.length - 1));

        if (typeof options?.speed === 'number' && Number.isFinite(options.speed)) {
            rsvpSpeed = Math.max(RSVP_MIN_SPEED, Math.min(RSVP_MAX_SPEED, Math.round(options.speed)));
            const speedLabel = document.getElementById(RSVP_SPEED_ID);
            if (speedLabel) {
                speedLabel.textContent = String(rsvpSpeed);
            }
        }

        rsvpPlaying = options?.autoPlay === true;
        const btn = document.getElementById(RSVP_PLAY_BUTTON_ID);
        if (btn) {
            btn.innerHTML = rsvpPlaying
                ? '<svg width="28" height="28" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2"><rect x="6" y="4" width="4" height="16"/><rect x="14" y="4" width="4" height="16"/></svg>'
                : '<svg width="28" height="28" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2"><polygon points="5,3 19,12 5,21"/></svg>';
        }

        renderRsvpWord();
        if (rsvpPlaying) {
            scheduleRsvpAdvance();
        }
    },
    setRsvpWords(words, options) {
        const nextWords = Array.isArray(words)
            ? words.filter(word => typeof word === 'string' && word.trim().length > 0)
            : [];

        if (!nextWords.length) {
            return;
        }

        this.setRsvpTimeline(nextWords.map(word => ({
            word,
            durationMs: Math.round(60000 / Math.max((options?.speed || rsvpSpeed), 1)),
            pauseAfterMs: 0,
            baseWpm: options?.speed || rsvpSpeed,
            nextPhrase: '',
            emotion: RSVP_NEUTRAL_EMOTION
        })), options);
    }
};
