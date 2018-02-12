using UnityEngine;
using System.Collections;

namespace Crosstales.BWF.Test
{
    /// <summary>Base class for all tests.</summary>
    public abstract class BaseTest : MonoBehaviour
    {
        #region Variables

        //Inspector variables
        public int Iterations = 50;
        public int TextStartLength = 100;
        public int TextGrowPerIteration = 0;

        public Model.ManagerMask[] Managers;
        public string[] TestSources;

        public string RandomChars = "0123456789abcdefghijklmnopqrstuvwxyzABCDEFGHIJKLMNOPQRSTUVWXYZ.?!-*";

        public char ReplaceChar = '*';

        //public bool Debugging = false;

        //private variables
        protected System.Random rd = new System.Random();

        protected System.Diagnostics.Stopwatch stopWatch = new System.Diagnostics.Stopwatch();

        protected int failCounter = 0;
        //protected float totalTime = 0f;

        protected static readonly string badword = "Fuuuccckkk";
        protected static readonly string noBadword = "assume";
        protected static readonly string domain = "goOgle.cOm";
        protected static readonly string email = "stEve76@goOgle.cOm";
        protected static readonly string noDomain = "my.cOmMand";
        protected static readonly string scunthorpe = "scuntHorPe";
        protected static readonly string arabicBadword = @"آنتاكلب";
        protected static readonly string globalBadword = "h!+leR";
        protected static readonly string nameBadword = "bAmbi";
        protected static readonly string emoji = "卍";

        private bool isFirsttime = true;

        #endregion


        #region MonoBehaviour methods

        public virtual void Update()
        {
            if (isFirsttime)
            {
                StartCoroutine(runTest());
                isFirsttime = false;
            }
        }

        #endregion


        #region Protected methods

        protected virtual IEnumerator runTest()
        {
            Debug.Log("*** '" + this.name + "' started. ***");

            while (!BWFManager.isReady)
            {
                //Debug.Log("Waiting");
                yield return null;
            }

            //setup the managers
            Crosstales.BWF.Manager.BadWordManager.ReplaceCharacters = new string(ReplaceChar, 1);
            Crosstales.BWF.Manager.DomainManager.ReplaceCharacters = new string(ReplaceChar, 1);

            //         Debug.Log("Sources");
            //         foreach(string source in BadWordManager.GetSources()) {
            //            Debug.Log("Source: " + source);
            //         }

            foreach (Model.ManagerMask mask in Managers)
            {
                speedTest(mask);
                yield return null;

                sanityTest(mask);
                yield return null;
            }

            if (failCounter > 0)
            {
                Debug.LogError("--- '" + this.name + "' ended with failures: " + failCounter + " ---");
            }
            else
            {
                Debug.Log("+++ '" + this.name + "' successfully ended. +++");
            }
        }

        protected virtual string createRandomString(int stringLength)
        {
            char[] chars = new char[stringLength];

            for (int ii = 0; ii < stringLength; ii++)
            {
                chars[ii] = RandomChars[rd.Next(0, RandomChars.Length)];
            }

            return new string(chars);
        }

        protected abstract void speedTest(Model.ManagerMask mask);

        protected abstract void sanityTest(Model.ManagerMask mask);

        #endregion
    }
}
// © 2015-2017 crosstales LLC (https://www.crosstales.com)