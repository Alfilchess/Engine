using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.IO;

public class MonteCarlo
{
	public class Move
	{
		public string move;
		public int index;
		public float score;
		public float proba;

		public Move(string move,int index,float score)
		{
			this.move = move;
			this.index = index;
			this.score = score;
			this.proba = 0;
		}

		public string MoveAndScore
		{
			get
			{
				return move + "\t" + score;
			}
		}

		public override string ToString()
		{
			return move+ " | "+score.ToString("N02")+ " | " + (proba*100).ToString("N02")+"%";
		}
	}
	static List<Move> moves = new List<Move>();

	static Dictionary<ulong, ulong[]> binomialCoefficients = new Dictionary<ulong, ulong[]>();

	//Binomial & Bernstein
	public static ulong BinomialCoefficient(ulong n, ulong k)
	{
		ulong[] coefArray;
		if (!binomialCoefficients.TryGetValue(n, out coefArray))
		{
			coefArray = new ulong[n / 2 + 1];
			ulong c = 1;
			for (ulong i = 0; i <= n / 2; i++)
			{
				coefArray[i] = c;
				c = c * (n - i);
				c = c / (i + 1);
			}
			binomialCoefficients.Add(n, coefArray);
		}

		if (k > n - k)
		{
			k = n - k;
		}

		return coefArray[k];
	}


	public static float GetBernsteinValue(ulong m, ulong i, float t)
	{
		//B (m,i,t) = C(m,i) * t^i * (1-t)^(m-i)
		// where C(m,i) = m!/(i!*(m-i)!)  ... is the binomial coefficient

		if (i > m) return float.NaN;

		return (float)( BinomialCoefficient(m, i) * Math.Pow(t, i) * Math.Pow(1 - t, m - i));
	}

	// 
	public static float GetFilterValue(float C, float k, float t)
	{
		k = Math.Max(Math.Abs(k), 0.00001f) * Math.Sign(k);

		//function f: [0,1] -> [0,1]
		// a = (1-C)/(1-exp(k))
		// f(t) = 1+a*(exp(k*t)-1)

		t = Math.Max(Math.Min(t, 1), 0);
		C = Math.Max(Math.Min(C, 1), 0);

		double a = (1 - C) / (1 - Math.Exp(k));

		return (float)(1 + a * (Math.Exp(k * t) - 1));
	}

	public static void NormalizeArrayMinMax(ref float[] toBeNormalizedArray)
	{
		float min = float.MaxValue;
		float max = float.MinValue;

		for (int i = 0; i < toBeNormalizedArray.Length; i++)
		{
			min = Math.Min(min, toBeNormalizedArray[i]);
			max = Math.Max(max, toBeNormalizedArray[i]);
		}

		float amplitude = max - min;

		for (int i = 0; i < toBeNormalizedArray.Length; i++)
			toBeNormalizedArray[i] = amplitude > 0 ? (toBeNormalizedArray[i] - min) / amplitude : 0;
	}

	public static void NormalizeArray0Max(ref float[] toBeNormalizedArray)
	{
		float max = float.MinValue;

		for (int i = 0; i < toBeNormalizedArray.Length; i++)
			max = Math.Max(max, Math.Abs(toBeNormalizedArray[i]));

		for (int i = 0; i < toBeNormalizedArray.Length; i++)
			toBeNormalizedArray[i] = max > 0 ? toBeNormalizedArray[i] / max : 0;
	}

	public static uint[] GetMovesOccurences(float[] movesScores, // ordered descending
											float relAILevel, // 0 <= relAILevel <= 1 
															  //float bernsteinOffset,
															  //float bernsteinScale,
											float filterK,  // must not be null
											float filterC       // 0 <= C < 1
		)
	{
		if (movesScores == null
			|| movesScores.Length == 0
			|| relAILevel < 0
			|| relAILevel > 1
			|| filterK == 0
			|| filterC < 0
			|| filterC > 1)
		{
			//Debug.LogError("GetSelectedMoveIndex ... error with one of the parameters");
			return null;
		}

		if (movesScores.Length == 1)
			return new uint[1];

		float[] normScores = Array.ConvertAll(movesScores, x => (float)x);
		NormalizeArrayMinMax(ref normScores);     //	normalized scores

		float[] probaValues = new float[movesScores.Length];    //	probability values

		//Preparation of the Probability value with Bernstein coefficients
		for (uint i = 0; i < movesScores.Length; i++)
			probaValues[i] = GetBernsteinValue((ulong)(movesScores.Length - 1), (ulong)i, 1 - relAILevel);// *bernsteinScale + bernsteinOffset;

		
		//Final proba computation ... we apply the filter to give more weight to the higher values
		for (uint i = 0; i < movesScores.Length; i++)
		{
			float filterValue = GetFilterValue(filterC, filterK, 1 - normScores[i]);
			probaValues[i] *= filterValue;
		}

		NormalizeArray0Max(ref probaValues);
		

		uint maxNumberOfoccurencesForOneProbaValue = (uint)Math.Pow(10, Math.Max(2, (uint)Math.Log10(movesScores.Length) + 1));

		//let's compute the Number of Occurences
		uint totalNumberOfOccurences = 0;
		uint[] numberOfOccurences = new uint[movesScores.Length];

		for (uint i = 0; i < movesScores.Length; i++)
		{
			numberOfOccurences[i] = (uint)(probaValues[i] * maxNumberOfoccurencesForOneProbaValue);
			totalNumberOfOccurences += numberOfOccurences[i];
		}

		//Let's compute the occurences
		uint[] occurences = new uint[totalNumberOfOccurences];
		uint index = 0;
		for (uint i = 0; i < movesScores.Length; i++)
		{
			for (uint j = 0; j < numberOfOccurences[i]; j++)
			{
				occurences[index++] = i;
			}
		}

		return occurences;
	}

	public static uint[] GetMovesOccurences2(float[] movesScores, // ordered descending
											float relAILevel, // 0 <= relAILevel <= 1 
															  //float bernsteinOffset,
															  //float bernsteinScale,
											float minFilterK,  // must not be null
											float filterC       // 0 <= C < 1
		)
	{
		float maxFilterK = -.001f;
		minFilterK = Math.Min(minFilterK, maxFilterK);
		float filterK = maxFilterK + relAILevel * (minFilterK -maxFilterK);

		if (movesScores == null
			|| movesScores.Length == 0
			|| relAILevel < 0
			|| relAILevel > 1
			|| filterK == 0
			|| filterC < 0
			|| filterC > 1)
		{
			//Debug.LogError("GetSelectedMoveIndex ... error with one of the parameters");
			return null;
		}

		if (movesScores.Length == 1)
			return new uint[1];

		float[] normScores = Array.ConvertAll(movesScores, x => (float)x);
		NormalizeArrayMinMax(ref normScores);     //	normalized scores

		float[] probaValues = new float[movesScores.Length];    //	probability values

		//Final proba computation ... we apply the filter to give more weight to the higher values
		for (uint i = 0; i < movesScores.Length; i++)
		{
			float filterValue = GetFilterValue(filterC, filterK, 1 - normScores[i]);
			probaValues[i] = filterValue;
		}

		NormalizeArray0Max(ref probaValues);

		uint maxNumberOfoccurencesForOneProbaValue = (uint)Math.Pow(10, Math.Max(2, (uint)Math.Log10(movesScores.Length) + 1));

		//let's compute the Number of Occurences
		uint totalNumberOfOccurences = 0;
		uint[] numberOfOccurences = new uint[movesScores.Length];

		for (uint i = 0; i < movesScores.Length; i++)
		{
			numberOfOccurences[i] = (uint)(probaValues[i] * maxNumberOfoccurencesForOneProbaValue);
			totalNumberOfOccurences += numberOfOccurences[i];
		}

		//Let's compute the occurences
		uint[] occurences = new uint[totalNumberOfOccurences];
		uint index = 0;
		for (uint i = 0; i < movesScores.Length; i++)
		{
			for (uint j = 0; j < numberOfOccurences[i]; j++)
			{
				occurences[index++] = i;
			}
		}

		return occurences;
	}
	public static int GetRandomSelection(uint[] occurences)
	{
		if (occurences != null && occurences.Length > 0)
			return (int)occurences[new Random().Next(0, occurences.Length)];
		else return -1;

	}

  // Use this for initialization
  //--------------------------------------------------------------------------- 
  public static int Find(float nLevel, List<float> nonOrderedMovesScores,  float pfilterK = -2f, float pfilterC = .1f)
  {
		if (nonOrderedMovesScores == null) return -1;
		if (nonOrderedMovesScores.Count == 1) return 0;

		moves.Clear();
		for (int i = 0; i < nonOrderedMovesScores.Count; i++)
		{
			moves.Add(new Move("",i, nonOrderedMovesScores[i]));
		}

		moves.Sort((a, b) => { return b.score.CompareTo(a.score); });

		float[] orderedMovesScores = new float[moves.Count];
		for (int i = 0; i < moves.Count; i++)
		{
			orderedMovesScores[i] = moves[i].score;
		}

		uint[] occurences = GetMovesOccurences2(orderedMovesScores,
                              relAILevel: Math.Min(Math.Max((nLevel-1) / 15,0),1),
                              minFilterK: pfilterK,
                              filterC: pfilterC);


    int nIndex = moves[GetRandomSelection(occurences)].index;

    return nIndex;
  }

	public static int Find(float nLevel, List<Move> nonOrderedMoves, float pfilterK = -2f, float pfilterC = .1f)
	{
		if (nonOrderedMoves == null) return -1;
		if (nonOrderedMoves.Count == 1) return 0;

		moves = nonOrderedMoves;
		moves.Sort((a, b) => { return b.score.CompareTo(a.score); });

		float[] orderedMovesScores = new float[moves.Count];
		for (int i = 0; i < moves.Count; i++)
		{
			orderedMovesScores[i] = moves[i].score;
		}

		uint[] occurences = GetMovesOccurences2(orderedMovesScores,
							  relAILevel: Math.Min(Math.Max((nLevel - 1) / 15, 0), 1),
							  minFilterK: pfilterK,
							  filterC: pfilterC);

		for (int i = 0; i < occurences.Length; i++)
			moves[(int)occurences[i]].proba++;

		for (int i = 0; i < moves.Count; i++)
			moves[i].proba /= occurences.Length;

		string str = "\n"+DateTime.Now.ToString()+"\n";
		str += "aiLevel = "+nLevel+" filterK = " + pfilterK + "  filterC = " + pfilterC+"\n";

		for (int i = 0; i < moves.Count; i++)
		{
			str += moves[i].ToString()+ "\n";
		}

		/*
		for (int i = 0; i < moves.Count; i++)
		{
			str += moves[i].MoveAndScore+"\n";
		}
		for (int i = 0; i < moves.Count; i++)
		{
			str += (moves[i].proba * 100).ToString("N02") + "\n";
		}
		*/

		int nIndex = moves[GetRandomSelection(occurences)].index;

		File.AppendAllText("LogAiceLatestMoveProbabilities.txt", str);


		return nIndex;
	}
}
