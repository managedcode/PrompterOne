namespace ManagedCode.Tps;

public static partial class TpsSpec
{
    public static class ArchetypeRecommendedWpmValues
    {
        public const int Friend = 135;
        public const int Motivator = 155;
        public const int Educator = 120;
        public const int Coach = 145;
        public const int Storyteller = 125;
        public const int Entertainer = 150;
    }

    public static class ArchetypeRangeValues
    {
        public static readonly NumericRange FriendEnergy = new(4, 6);
        public static readonly NumericRange FriendMelody = new(6, 8);
        public static readonly NumericRange FriendSpeed = new(125, 150);
        public static readonly NumericRange MotivatorEnergy = new(7, 10);
        public static readonly NumericRange MotivatorMelody = new(7, 9);
        public static readonly NumericRange MotivatorSpeed = new(145, 170);
        public static readonly NumericRange EducatorEnergy = new(3, 5);
        public static readonly NumericRange EducatorMelody = new(2, 4);
        public static readonly NumericRange EducatorSpeed = new(110, 135);
        public static readonly NumericRange CoachEnergy = new(7, 9);
        public static readonly NumericRange CoachMelody = new(1, 3);
        public static readonly NumericRange CoachSpeed = new(135, 160);
        public static readonly NumericRange StorytellerEnergy = new(4, 7);
        public static readonly NumericRange StorytellerMelody = new(8, 10);
        public static readonly NumericRange StorytellerSpeed = new(100, 150);
        public static readonly NumericRange EntertainerEnergy = new(6, 8);
        public static readonly NumericRange EntertainerMelody = new(7, 9);
        public static readonly NumericRange EntertainerSpeed = new(140, 165);
        public static readonly NumericRange FriendPhraseLength = new(8, 15);
        public static readonly NumericRange FriendPauseFrequency = new(4, 8);
        public static readonly NumericRange FriendPauseDuration = new(300, 600);
        public static readonly NumericRange FriendEmphasisDensity = new(3, 8);
        public static readonly NumericRange FriendSpeedVariation = new(0, 1);
        public static readonly NumericRange MotivatorPhraseLength = new(8, 20);
        public static readonly NumericRange MotivatorPauseFrequency = new(3, 6);
        public static readonly NumericRange MotivatorPauseDuration = new(600, 2000);
        public static readonly NumericRange MotivatorEmphasisDensity = new(10, 20);
        public static readonly NumericRange MotivatorSpeedVariation = new(0, 2);
        public static readonly NumericRange EducatorPhraseLength = new(10, 25);
        public static readonly NumericRange EducatorPauseFrequency = new(6, 12);
        public static readonly NumericRange EducatorPauseDuration = new(400, 800);
        public static readonly NumericRange EducatorEmphasisDensity = new(3, 8);
        public static readonly NumericRange EducatorSpeedVariation = new(0, 2);
        public static readonly NumericRange CoachPhraseLength = new(3, 8);
        public static readonly NumericRange CoachPauseFrequency = new(8, 15);
        public static readonly NumericRange CoachPauseDuration = new(200, 400);
        public static readonly NumericRange CoachEmphasisDensity = new(15, 30);
        public static readonly NumericRange CoachSpeedVariation = new(0, 2);
        public static readonly NumericRange StorytellerPhraseLength = new(5, 20);
        public static readonly NumericRange StorytellerPauseFrequency = new(4, 10);
        public static readonly NumericRange StorytellerPauseDuration = new(500, 3000);
        public static readonly NumericRange StorytellerEmphasisDensity = new(5, 12);
        public static readonly NumericRange StorytellerSpeedVariation = new(3, 6);
        public static readonly NumericRange EntertainerPhraseLength = new(5, 15);
        public static readonly NumericRange EntertainerPauseFrequency = new(5, 10);
        public static readonly NumericRange EntertainerPauseDuration = new(300, 2000);
        public static readonly NumericRange EntertainerEmphasisDensity = new(5, 15);
        public static readonly NumericRange EntertainerSpeedVariation = new(2, 4);
    }
}
