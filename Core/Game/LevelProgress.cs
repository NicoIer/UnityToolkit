using System;

namespace UnityToolkit
{
    public sealed class LevelProgress
    {
        /// <summary>
        /// level等级到level+1所需的经验
        /// </summary>
        public delegate int TotalExpGetter(int level);

        public delegate void OnAttainExpDelegate(int added, int currentLevelExp, int nextLevelExp);

        public delegate void OnLevelUpDelegate(int oldLevel, int newLevel,int currentExp,int nextLevelExp);

        private readonly TotalExpGetter _totalExpGetter;
        public event OnAttainExpDelegate OnAttainExp = delegate { };
        public event OnLevelUpDelegate OnLevelUp = delegate { };
        public int currentLevel { get; private set; }
        public int totalExp { get; private set; }
        public int currentLevelTotalExp => _totalExpGetter(currentLevel);
        public int nextLevelTotalExp => _totalExpGetter(currentLevel + 1);
        public int neededExpForNextLevel => nextLevelTotalExp - totalExp;

        public float nextLevelProgress { get; private set; }

        public bool atMaxLevel { get; private set; }
        public int maxLevel { get; private set; }

        public LevelProgress(int currentLevel, int maxLevel, TotalExpGetter totalExpGetter)
        {
            _totalExpGetter = totalExpGetter;
            this.maxLevel = maxLevel;
            this.currentLevel = Math.Clamp(currentLevel, 1, maxLevel);
            totalExp = currentLevelTotalExp;
        }

        public void AddExp(int exp)
        {
            if (atMaxLevel) return;
            while (exp > neededExpForNextLevel && exp > 0)
            {
                AttainExp(neededExpForNextLevel);
                exp -= neededExpForNextLevel;
                LevelUp();
            }

            // 加上剩下的经验
            AttainExp(exp);
        }

        private void AttainExp(int exp)
        {
            totalExp += exp;
            nextLevelProgress = totalExp - currentLevelTotalExp / ((float)nextLevelTotalExp - currentLevelTotalExp);
            OnAttainExp(exp, totalExp - currentLevelTotalExp, nextLevelTotalExp - currentLevelTotalExp);
        }

        private void LevelUp()
        {
            if(atMaxLevel) return; 
            currentLevel++;
            totalExp = currentLevelTotalExp;
            if (currentLevel >= maxLevel)
            {
                atMaxLevel = true;
                currentLevel = maxLevel;
            }

            OnLevelUp(currentLevel - 1, currentLevel, totalExp - currentLevelTotalExp, nextLevelTotalExp - currentLevelTotalExp);
        }
    }
}