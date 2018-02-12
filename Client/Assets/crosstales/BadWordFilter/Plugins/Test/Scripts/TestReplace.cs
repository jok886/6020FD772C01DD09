using UnityEngine;

namespace Crosstales.BWF.Test
{
    /// <summary>Test for the 'Replace' method.</summary>
    [HelpURL("https://www.crosstales.com/media/data/assets/badwordfilter/api/class_crosstales_1_1_b_w_f_1_1_test_1_1_test_replace.html")]
    public class TestReplace : BaseTest
    {
        #region Implemented methods

        protected override void speedTest(Model.ManagerMask mask)
        {
            System.Collections.Generic.List<string> badWords = new System.Collections.Generic.List<string>();
            badWords.Add(badword);

            stopWatch.Reset();
            stopWatch.Start();

            for (int ii = 0; ii < Iterations; ii++)
            {
                BWFManager.Replace(createRandomString(TextStartLength + (TextGrowPerIteration * ii)), badWords, mask);
            }

            stopWatch.Stop();

            //Debug.Log(totalTime + ";" + totalTime/Iterations);
            Debug.Log("## " + mask + ": " + stopWatch.ElapsedMilliseconds + ";" + ((float)stopWatch.ElapsedMilliseconds / Iterations) + " ##");
        }

        protected override void sanityTest(Model.ManagerMask mask)
        {
            System.Collections.Generic.List<string> noWords = new System.Collections.Generic.List<string>();

            //null test
            if (BWFManager.Replace(null, noWords, mask).Equals(string.Empty))
            {
                if (Util.Config.DEBUG)
                    Debug.Log("Nullable 'text test passed");
            }
            else
            {
                Debug.LogError("Nullable 'text' test failed");
                failCounter++;
            }

            if (BWFManager.Replace(scunthorpe, null, mask).Equals(scunthorpe))
            {
                if (Util.Config.DEBUG)
                    Debug.Log("Nullable 'badWords test passed");
            }
            else
            {
                Debug.LogError("Nullable 'badWords' test failed");
                failCounter++;
            }

            //empty test
            if (BWFManager.Replace(string.Empty, noWords, mask).Equals(string.Empty))
            {
                if (Util.Config.DEBUG)
                    Debug.Log("Empty test passed");
            }
            else
            {
                Debug.LogError("Empty test failed");
                failCounter++;
            }

            if ((mask & Model.ManagerMask.BadWord) == Model.ManagerMask.BadWord || (mask & Model.ManagerMask.All) == Model.ManagerMask.All)
            {
                System.Collections.Generic.List<string> badWords = new System.Collections.Generic.List<string>();
                badWords.Add(badword);

                //replace test
                if (BWFManager.Replace(badword, badWords, mask).Equals(new string(ReplaceChar, badword.Length)))
                {
                    if (Util.Config.DEBUG)
                        Debug.Log("Bad word resource replace test passed");
                }
                else
                {
                    Debug.LogError("Bad word resource replace failed");
                    failCounter++;
                }
            }

            if ((mask & Model.ManagerMask.Domain) == Model.ManagerMask.Domain || (mask & Model.ManagerMask.All) == Model.ManagerMask.All)
            {
                System.Collections.Generic.List<string> domainWords = new System.Collections.Generic.List<string>();
                domainWords.Add(domain);

                //domain match test
                if (BWFManager.Replace(domain, domainWords, mask).Equals(new string(ReplaceChar, domain.Length)))
                {
                    if (Util.Config.DEBUG)
                        Debug.Log("Domain replace test passed");
                }
                else
                {
                    Debug.LogError("Domain match failed");
                    failCounter++;
                }
            }

            if ((mask & Model.ManagerMask.Capitalization) == Model.ManagerMask.Capitalization)
            {
                string caps = new string('A', Crosstales.BWF.Manager.CapitalizationManager.CharacterNumber + 1);
                System.Collections.Generic.List<string> capsWords = new System.Collections.Generic.List<string>();
                capsWords.Add(caps);

                //capital match test
                if (BWFManager.Replace(caps, capsWords, mask).Equals(caps.ToLowerInvariant()))
                {
                    if (Util.Config.DEBUG)
                        Debug.Log("Capital replace test passed");
                }
                else
                {
                    Debug.LogError("Capital replace failed");
                    failCounter++;
                }
            }

            if ((mask & Model.ManagerMask.Punctuation) == Model.ManagerMask.Punctuation)
            {
                string punc = new string('!', Crosstales.BWF.Manager.PunctuationManager.CharacterNumber + 1);
                System.Collections.Generic.List<string> puncWords = new System.Collections.Generic.List<string>();
                puncWords.Add(punc);

                //punctuation match test
                if (BWFManager.Replace(punc, puncWords, mask).Equals(new string('!', Crosstales.BWF.Manager.PunctuationManager.CharacterNumber)))
                {
                    if (Util.Config.DEBUG)
                        Debug.Log("Punctuation replace test passed");
                }
                else
                {
                    Debug.LogError("Punctuation replace failed");
                    failCounter++;
                }
            }
        }

        #endregion
    }
}
// © 2015-2017 crosstales LLC (https://www.crosstales.com)