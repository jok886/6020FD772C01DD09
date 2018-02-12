using UnityEngine;

namespace Crosstales.BWF.Test
{
    /// <summary>Test for the 'ReplaceAll()' method.</summary>
    [HelpURL("https://www.crosstales.com/media/data/assets/badwordfilter/api/class_crosstales_1_1_b_w_f_1_1_test_1_1_test_replace_all.html")]
    public class TestReplaceAll : BaseTest
    {
        #region Implemented methods

        protected override void speedTest(Model.ManagerMask mask)
        {
            stopWatch.Reset();
            stopWatch.Start();

            for (int ii = 0; ii < Iterations; ii++)
            {
                BWFManager.ReplaceAll(createRandomString(TextStartLength + (TextGrowPerIteration * ii)), mask, TestSources);
            }

            stopWatch.Stop();

            //Debug.Log(totalTime + ";" + totalTime/Iterations);
            Debug.Log("## " + mask + ": " + stopWatch.ElapsedMilliseconds + ";" + ((float)stopWatch.ElapsedMilliseconds / Iterations) + " ##");
        }

        protected override void sanityTest(Model.ManagerMask mask)
        {
            //null test
            //         Debug.Log("MASK: " + mask);
            //
            //         Debug.Log("REPLACE normal: '" + MultiManager.ReplaceAll(null, ManagerMask.All) + "'");

            if (BWFManager.ReplaceAll(null, mask).Equals(string.Empty))
            {
                if (Util.Config.DEBUG)
                    Debug.Log("Nullable test passed");
            }
            else
            {
                Debug.LogError("Nullable test failed");
                failCounter++;
            }

            //empty test
            if (BWFManager.ReplaceAll(string.Empty, mask).Equals(string.Empty))
            {
                if (Util.Config.DEBUG)
                    Debug.Log("Empty test passed");
            }
            else
            {
                Debug.LogError("Empty test failed");
                failCounter++;
            }

            if ((mask & Model.ManagerMask.Domain) == Model.ManagerMask.Domain || (mask & Model.ManagerMask.BadWord) == Model.ManagerMask.BadWord || (mask & Model.ManagerMask.All) == Model.ManagerMask.All)
            {
                //wrong 'source' test
                if (BWFManager.ReplaceAll(scunthorpe, mask, "test").Equals(scunthorpe))
                {
                    if (Util.Config.DEBUG)
                        Debug.Log("Wrong 'source' test passed");
                }
                else
                {
                    Debug.LogError("Wrong 'source' test failed");
                    failCounter++;
                }

                //null for 'source' test
                if (BWFManager.ReplaceAll(scunthorpe, mask, null).Equals(scunthorpe))
                {
                    if (Util.Config.DEBUG)
                        Debug.Log("Null for 'source' test passed");
                }
                else
                {
                    Debug.LogError("Null for 'source' test failed");
                    failCounter++;
                }

                //Zero-length array for 'source' test
                if (BWFManager.ReplaceAll(scunthorpe, mask, new string[0]).Equals(scunthorpe))
                {
                    if (Util.Config.DEBUG)
                        Debug.Log("Zero-length array for 'source' test passed");
                }
                else
                {
                    Debug.LogError("Zero-length array for 'source' test failed");
                    failCounter++;
                }
            }

            if ((mask & Model.ManagerMask.BadWord) == Model.ManagerMask.BadWord || (mask & Model.ManagerMask.All) == Model.ManagerMask.All)
            {
                //normal bad word match test
                if (BWFManager.ReplaceAll(badword, mask).Equals(new string(ReplaceChar, badword.Length)))
                {
                    if (Util.Config.DEBUG)
                        Debug.Log("Normal bad word replace test passed");
                }
                else
                {
                    Debug.LogError("Normal bad word replace test failed");
                    failCounter++;
                }

                //normal bad word non-match test
                if (BWFManager.ReplaceAll(scunthorpe, mask).Equals(scunthorpe))
                {
                    if (Util.Config.DEBUG)
                        Debug.Log("Normal bad word non-replace test passed");
                }
                else
                {
                    Debug.LogError("Normal bad word non-replace word test failed");
                    failCounter++;
                }

                //bad word resource match test
                if (BWFManager.ReplaceAll(badword, mask, "english").Equals(new string(ReplaceChar, badword.Length)))
                {
                    if (Util.Config.DEBUG)
                        Debug.Log("Bad word resource replace test passed");
                }
                else
                {
                    Debug.LogError("Bad word resource replace failed");
                    failCounter++;
                }

                //bad word resource non-match test
                if (BWFManager.ReplaceAll(noBadword, mask, "english").Equals(noBadword))
                {
                    if (Util.Config.DEBUG)
                        Debug.Log("Bad word resource non-replace test passed");
                }
                else
                {
                    Debug.LogError("Bad word resource non-replace word test failed");
                    failCounter++;
                }

                //arabic match test
                if (BWFManager.ReplaceAll(arabicBadword, mask).Equals(new string(ReplaceChar, arabicBadword.Length)))
                {
                    if (Util.Config.DEBUG)
                        Debug.Log("Arabic bad word replace test passed");
                }
                else
                {
                    Debug.LogError("Arabic bad word replace failed");
                    failCounter++;
                }

                //global match test
                if (BWFManager.ReplaceAll(globalBadword, mask).Equals(new string(ReplaceChar, globalBadword.Length)))
                {
                    if (Util.Config.DEBUG)
                        Debug.Log("Global bad word replace test passed");
                }
                else
                {
                    Debug.LogError("Global bad word replace failed");
                    failCounter++;
                }

                //name match test
                if (BWFManager.ReplaceAll(nameBadword, mask).Equals(new string(ReplaceChar, nameBadword.Length)))
                {
                    if (Util.Config.DEBUG)
                        Debug.Log("Name replace test passed");
                }
                else
                {
                    Debug.LogError("Name replace failed");
                    failCounter++;
                }

                //emoji match test
                if (BWFManager.ReplaceAll(emoji, mask).Equals(new string(ReplaceChar, emoji.Length)))
                {
                    if (Util.Config.DEBUG)
                        Debug.Log("Emoji replace test passed");
                }
                else
                {
                    Debug.LogError("Emoji replace failed");
                    failCounter++;
                }
            }

            if ((mask & Model.ManagerMask.Domain) == Model.ManagerMask.Domain || (mask & Model.ManagerMask.All) == Model.ManagerMask.All)
            {
                //domain match test
                if (BWFManager.ReplaceAll(domain, mask).Equals(new string(ReplaceChar, domain.Length)))
                {
                    if (Util.Config.DEBUG)
                        Debug.Log("Domain replace test passed");
                }
                else
                {
                    Debug.LogError("Domain replace failed");
                    failCounter++;
                }

                //email match test
                if (BWFManager.ReplaceAll(email, mask).Equals(new string(ReplaceChar, email.Length)))
                {
                    if (Util.Config.DEBUG)
                        Debug.Log("Email replace test passed");
                }
                else
                {
                    Debug.LogError("Email replace failed");
                    failCounter++;
                }

                //domain non-match test
                if (BWFManager.ReplaceAll(noDomain, mask).Equals(noDomain))
                {
                    if (Util.Config.DEBUG)
                        Debug.Log("Domain non-replace test passed");
                }
                else
                {
                    Debug.LogError("Domain non-replace word test failed");
                    failCounter++;
                }
            }

            if ((mask & Model.ManagerMask.Capitalization) == Model.ManagerMask.Capitalization)
            {
                //capital match test
                string caps = new string('A', Crosstales.BWF.Manager.CapitalizationManager.CharacterNumber + 1);
                string noCaps = new string('A', Crosstales.BWF.Manager.CapitalizationManager.CharacterNumber);

                if (BWFManager.ReplaceAll(caps, mask).Equals(caps.ToLowerInvariant()))
                {
                    if (Util.Config.DEBUG)
                        Debug.Log("Capital replace test passed");
                }
                else
                {
                    Debug.LogError("Capital replace failed");
                    failCounter++;
                }

                //capital non-match test
                if (BWFManager.ReplaceAll(noCaps, mask).Equals(noCaps))
                {
                    if (Util.Config.DEBUG)
                        Debug.Log("Capital non-replace test passed");
                }
                else
                {
                    Debug.LogError("Capital non-replace word test failed");
                    failCounter++;
                }
            }

            if ((mask & Model.ManagerMask.Punctuation) == Model.ManagerMask.Punctuation)
            {
                //punctuation match test
                string punc = new string('!', Crosstales.BWF.Manager.PunctuationManager.CharacterNumber + 1);
                string noPunc = new string('!', Crosstales.BWF.Manager.PunctuationManager.CharacterNumber);

                if (BWFManager.ReplaceAll(punc, mask).Equals(noPunc))
                {
                    if (Util.Config.DEBUG)
                        Debug.Log("Punctuation replace test passed");
                }
                else
                {
                    Debug.LogError("Punctuation replace failed");
                    failCounter++;
                }

                //punctuation non-match test
                if (BWFManager.ReplaceAll(noPunc, mask).Equals(noPunc))
                {
                    if (Util.Config.DEBUG)
                        Debug.Log("Punctuation non-replace test passed");
                }
                else
                {
                    Debug.LogError("Punctuation non-replace word test failed");
                    failCounter++;
                }
            }
        }

        #endregion
    }
}
// © 2015-2017 crosstales LLC (https://www.crosstales.com)