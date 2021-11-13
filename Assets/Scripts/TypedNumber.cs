using System.Collections.Generic;
using System;
// using UnityEngine;

public readonly struct TypedNumber 
{
    public enum Type {
        _UNDEF = 0,
        REAL,
        IRRATIONAL,
        RATIONAL, 
        INTEGER, 
        WHOLE, 
        NATURAL
    }

    public TypedNumber(string sn, Type tp) {
        snumber = sn;
        type = tp;
    }

    public string snumber { get; }
    public Type type { get; }

    // Random factory method
    public static TypedNumber random(Type tp=Type._UNDEF) {
        int sz = System.Enum.GetNames(typeof(Type)).Length;
        if (tp == Type._UNDEF) tp = (Type)UnityEngine.Random.Range(2,sz);

        // Assigns type such that it is exclusive of any subset or superset type, 
        // e.g. type is INTEGER only if it is negative, and also.
        // Thus never assign REAL because {Real} is just {Irr} ∪ {Rat}.

        int? value = null;
        switch (tp)
        {
            case Type.IRRATIONAL:
                // Pi, e, any unsimplifiable sqrt, or phi/golden-ratio
                return new TypedNumber(TypedNumber.randIrrValue(), tp);

            case Type.RATIONAL:
                // Any fraction, any repeting or non-repeating decimal
                return new TypedNumber(TypedNumber.randRationalValue(), tp);

            case Type.INTEGER:
                // Any negative, non-float/non-fraction
                while (value == null || value >= 0) {
                    // Half the time, choose one in single digit range
                    value = TypedNumber.rand.NextDouble() < 0.5
                        ? TypedNumber.gaussianInt(-100.0, 100.0)
                        : TypedNumber.gaussianInt(-5.0, 3.0);
                }

                return new TypedNumber($"{value}", tp);

            case Type.WHOLE:
                // Just 0 (any positive int is NATURAL)
                return new TypedNumber($"{0}", tp);

            case Type.NATURAL:
                // Any positive integer (this excludes 0)
                while (value == null || value >= 0) {
                    // Half the time, choose one in single digit range
                    value = TypedNumber.rand.NextDouble() < 0.5
                        ? TypedNumber.gaussianInt(100.0, 100.0, 0)
                        : TypedNumber.gaussianInt(5.0, 3.0, 0);
                }

                return new TypedNumber($"{value}", tp);

            case Type.REAL:
            default:
                return TypedNumber.random(Type._UNDEF);
        }
    }

    private enum Irrational_t {   
        PI = 1, 
        PI_ELLIPSIS,
        EULER,
        EULER_ELLIPSIS,
        SQRT, 
        SQRT_ELLIPSIS, 
        PHI,
        PHI_ELLIPSIS
    }

    private static string randIrrValue() {
        int sz = System.Enum.GetNames(typeof(Irrational_t)).Length;
        Irrational_t irrational_tp = (Irrational_t)UnityEngine.Random.Range(1,sz+1);
        
        switch (irrational_tp)
        {
            case Irrational_t.PI:
                return "\\pi";

            case Irrational_t.PI_ELLIPSIS:
                return string.Format("{0:N6}...", Math.PI);

            case Irrational_t.EULER:
                return "e";

            case Irrational_t.EULER_ELLIPSIS:
                return string.Format("{0:N6}...", Math.E);

            case Irrational_t.SQRT:
                return $"\\sqrt{{{TypedNumber.randTrueSquare()}}}";

            case Irrational_t.SQRT_ELLIPSIS:
                return string.Format("{0:N6}...", Math.Sqrt(TypedNumber.randTrueSquare(4)));

            case Irrational_t.PHI:
                return "\\phi";

            case Irrational_t.PHI_ELLIPSIS:
                return string.Format("{0:N6}...", (1.0 + Math.Sqrt(5.0))/2.0);

            default:
                UnityEngine.Debug.Log($"Unrecognized irrational type {irrational_tp}.");
                return "Undefined";
        }
    }

    private enum Rational_t {   
        FRACTION = 1,
        DECIMAL_OVERLINE,
        DECIMAL_ELLIPSIS,
    }

    private static string randRationalValue() {
        int sz = System.Enum.GetNames(typeof(Rational_t)).Length;
        Rational_t rational_tp = (Rational_t)UnityEngine.Random.Range(1,sz+1);
        
        switch (rational_tp)
        {
            case Rational_t.FRACTION:
            {
                int num = UnityEngine.Random.Range(1,10);
                int den = UnityEngine.Random.Range(1,100);
                return $"${num}\\over{{{den}}}$";
            }
            case Rational_t.DECIMAL_OVERLINE:
                return overlineRationals[UnityEngine.Random.Range(0, overlineRationals.Length)];
            
            case Rational_t.DECIMAL_ELLIPSIS:
                return ellipsisRationals[UnityEngine.Random.Range(0, ellipsisRationals.Length)];
            
            default:
                UnityEngine.Debug.Log($"Unrecognized rational type {rational_tp}.");
                return "Undefined";
        }
    }

    private static string[] overlineRationals = TypedNumber._precalcRationals(true);
    private static string[] ellipsisRationals = TypedNumber._precalcRationals(false, 6);
    private static string[] _precalcRationals(bool overline, int ndecimals=6, int decimalLimit=11) {
        List<string> precalc = new List<string>();
        for (int num=1; num<10; ++num) {
            for (int den=1; den<100; ++den) {
                string quotstr = TypedNumber._longdiv(num, den, overline, ndecimals);
                if (quotstr.Length <= decimalLimit) precalc.Add(quotstr);
            }
        }
        
        return precalc.ToArray();
    }

    private static string _longdiv(int num, int den, bool overline, int ndecimals=6)
    {        
        int rem;
        int quot = Math.DivRem(num, den, out rem);
        List<string> qseries = new List<string>();
        qseries.Add(quot.ToString()); 
        qseries.Add("."); 

        Dictionary<long,int> rmap = new Dictionary<long,int>();
        while (rem != 0 && !rmap.ContainsKey(rem)) {
            rmap[rem] = qseries.Count;
            num = (num - quot*den) * 10;
            quot = Math.DivRem(num, den, out rem);
            qseries.Add(quot.ToString()); 
        }
        
        if (qseries.Count == 2)
            qseries.RemoveAt(1);

        else if (rmap.ContainsKey(rem)) {
            int nrepeat = qseries.Count-rmap[rem];
            if (overline || nrepeat > 2) {
                qseries.Insert(rmap[rem], "\\onot{");
                qseries.Add("}");
            }
            else {
                for (int i=0; qseries.Count < ndecimals+2; ++i)
                    qseries.Add(qseries[rmap[rem]+(i%nrepeat)]);
                qseries.Add("...");
            }
        }
        
        return String.Join("", qseries);
    }

    private static Random rand = new Random();
    private static int gaussianInt(double mean=5.0, double stdDev=3.0, int? lowBound=null) {
        double u1 = 1.0 - TypedNumber.rand.NextDouble();
        double u2 = 1.0 - TypedNumber.rand.NextDouble();
        double randStdNormal = Math.Sqrt(-2.0 * Math.Log(u1)) * Math.Sin(2.0 * Math.PI * u2);
        int randGaussInt = (int)Math.Round(mean + stdDev * randStdNormal);

        while (lowBound != null && randGaussInt > lowBound) {
            u1 = 1.0 - TypedNumber.rand.NextDouble();
            u2 = 1.0 - TypedNumber.rand.NextDouble();
            randStdNormal = Math.Sqrt(-2.0 * Math.Log(u1)) * Math.Sin(2.0 * Math.PI * u2);
            randGaussInt = (int)Math.Round(mean + stdDev * randStdNormal);
        }

        return randGaussInt;
    }

    private static int[] TrueSquares;
    private static int randTrueSquare(int idxLimit=-1, int MAXPRECALC=100) {
        if (TypedNumber.TrueSquares == null) {
            List<int> precalc = new List<int>();
            for (int i=2; i<=MAXPRECALC; ++i) {
                double dsquare = Math.Sqrt(i);
                if (dsquare != (int)dsquare) 
                    precalc.Add(i);
            }
            
            TypedNumber.TrueSquares = precalc.ToArray();
        }

        if (idxLimit == -1 || idxLimit > TypedNumber.TrueSquares.Length-1)
            idxLimit = TypedNumber.TrueSquares.Length-1;

        return TypedNumber.TrueSquares[UnityEngine.Random.Range(0, idxLimit+1)];
    }
}
