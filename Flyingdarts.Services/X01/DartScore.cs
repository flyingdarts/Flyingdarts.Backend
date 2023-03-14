namespace Flyingdarts.Services.X01;

public class DartScore
{
    private int[] _values;

    /// <summary>
    /// <param name="Values">Contains the scores for either 3-dart-singles input or 3-dart-sum input </param>
    /// </summary>
    public int[] Values
    {
        get { return _values; }
        set
        {
            if (value.Length <= 3)
                _values = value;
        }
    }

    /// <summary>
    /// <param name="Score">Returns the total score of either 3-dart-singles input or 3-dart-sum </param>
    /// </summary>
    public int Score
    {
        get
        {
            return _values.Sum();
        }
    }
}