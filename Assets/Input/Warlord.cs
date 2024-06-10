using System;

[Serializable]
public class Warlord {
    public int Uid;
    public string Name = "Warlord";
    public int GamePoint;
    public double ExpGain;

    public UnitPoint HealthPoint;
    public UnitPoint StaminaPoint;
    public UnitPoint ManaPoint;
    public int AttackPower;
    public int DefendPower;
    public float AttackSpeed;
    //public float MoveSpeed;

    public Warlord(int _str, int _int, int _dex, int _men) {
        bonusStat.Initialize();
        InitializeStats(_str, _int, _dex, _men);
        AttackSpeed = 0.5f;
    }

    public int Level {
        get {
            //INT(E1^(1/4))
            var level = (int)Math.Pow(ExpGain, 0.25);
            InitializeStats(level);
            return (level > 1) ? level : 1;
        }
    }

    public double ExpNext {
        get {
            //(F1+1)^4
            return Math.Pow(Level + 1, 4);
        }
    }

    public (int, int) Strength {
        get {
            return (_str, bonusStat.Strength);
        }
        set {
            _str += value.Item1;
            bonusStat.Strength += value.Item2;
            if (_str + bonusStat.Strength > MAX_STAT) {
                bonusStat.Strength = MAX_STAT - _str;
            } else if (_str + bonusStat.Strength < 0) {
                bonusStat.Strength = 0;
            }
            HealthPoint.MaximumStock += (value.Item1 + value.Item2) * 350;
            StaminaPoint.MaximumStock += (value.Item1 + value.Item2) * 30;
            DefendPower += (value.Item1 + value.Item2) * 5;
        }
    }

    public (int, int) Intelligent {
        get {
            return (_int, bonusStat.Intelligent);
        }
        set {
            _int += value.Item1;
            bonusStat.Intelligent += value.Item2;
            if (_int + bonusStat.Intelligent > MAX_STAT) {
                bonusStat.Intelligent = MAX_STAT - _int;
            } else if (_int + bonusStat.Intelligent < 0) {
                bonusStat.Intelligent = 0;
            }
            StaminaPoint.MaximumStock += (value.Item1 + value.Item2) * 20;
            AttackPower += (value.Item1 + value.Item2) * 2;
        }
    }

    public (int, int) Dexetery {
        get {
            return (_dex, bonusStat.Dexetery);
        }
        set {
            _dex += value.Item1;
            bonusStat.Dexetery += value.Item2;
            if (_dex + bonusStat.Dexetery > MAX_STAT) {
                bonusStat.Dexetery = MAX_STAT - _dex;
            } else if (_dex + bonusStat.Dexetery < 0) {
                bonusStat.Dexetery = 0;
            }
            HealthPoint.MaximumStock += (value.Item1 + value.Item2) * 50;
            AttackPower += (value.Item1 + value.Item2) * 2;
        }
    }

    public (int, int) Mentality {
        get {
            return (_men, bonusStat.Mentality);
        }
        set {
            _men += value.Item1;
            bonusStat.Mentality += value.Item2;
            if (_men + bonusStat.Mentality > MAX_STAT) {
                bonusStat.Mentality = MAX_STAT - _men;
            } else if (_men + bonusStat.Mentality < 0) {
                bonusStat.Mentality = 0;
            }
            StaminaPoint.MaximumStock += (value.Item1 + value.Item2) * 10;
            ManaPoint.MaximumStock += (value.Item1 + value.Item2) * 50;
            DefendPower += (value.Item1 + value.Item2) * 2;
        }
    }

    private const int MAX_STAT = 99;

    private int _str;
    private int _int;
    private int _dex;
    private int _men;
    private BonusStat bonusStat;

    private void InitializeStats(params int[] args) {
        Strength = (args[0], 0);
        Intelligent = (args[1], 0);
        Dexetery = (args[2], 0);
        Mentality = (args[3], 0);
    }

    private void InitializeStats(int value) {
        Strength = (value, 0);
        Intelligent = (value, 0);
        Dexetery = (value, 0);
        Mentality = (value, 0);
    }
}

[Serializable]
public struct BonusStat {
    public int Strength;
    public int Intelligent;
    public int Dexetery;
    public int Mentality;

    public void Initialize() {
        Strength = 0;
        Intelligent = 0;
        Mentality = 0;
        Dexetery = 0;
    }
}