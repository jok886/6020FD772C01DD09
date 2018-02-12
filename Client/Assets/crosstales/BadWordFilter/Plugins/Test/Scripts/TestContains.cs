using UnityEngine;

namespace Crosstales.BWF.Test
{
    /// <summary>Test for the 'Contains()' method.</summary>
    [HelpURL("https://www.crosstales.com/media/data/assets/badwordfilter/api/class_crosstales_1_1_b_w_f_1_1_test_1_1_test_contains.html")]
    public class TestContains : BaseTest
    {

        #region Implemented methods

        protected override void speedTest(Model.ManagerMask mask)
        {
            stopWatch.Reset();
            stopWatch.Start();

            for (int ii = 0; ii < Iterations; ii++)
            {
                BWFManager.Contains(createRandomString(TextStartLength + (TextGrowPerIteration * ii)), mask, TestSources);
            }

            stopWatch.Stop();

            Debug.Log("## " + mask + ": " + stopWatch.ElapsedMilliseconds + ";" + ((float)stopWatch.ElapsedMilliseconds / Iterations) + " ##");
        }

        protected override void sanityTest(Model.ManagerMask mask)
        {
            //null test
            if (!BWFManager.Contains(null, mask))
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
            if (!BWFManager.Contains(string.Empty))
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
                if (!BWFManager.Contains(scunthorpe, mask, "test"))
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
                if (!BWFManager.Contains(scunthorpe, mask, null))
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
                if (!BWFManager.Contains(scunthorpe, mask, new string[0]))
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
                if (BWFManager.Contains(badword, mask))
                {
                    if (Util.Config.DEBUG)
                        Debug.Log("Normal bad word match test passed");
                }
                else
                {
                    Debug.LogError("Normal bad word match failed");
                    failCounter++;
                }

                //normal bad word non-match test
                if (!BWFManager.Contains(scunthorpe, mask))
                {
                    if (Util.Config.DEBUG)
                        Debug.Log("Normal bad word non-match test passed");
                }
                else
                {
                    Debug.LogError("Normal bad word non-match word test failed");
                    failCounter++;
                }
                //bad word resource match test
                if (BWFManager.Contains(badword, mask, "english"))
                {
                    if (Util.Config.DEBUG)
                        Debug.Log("Bad word resource match test passed");
                }
                else
                {
                    Debug.LogError("Bad word resource match failed");
                    failCounter++;
                }

                //bad word resource non-match test
                if (!BWFManager.Contains(noBadword, mask, "english"))
                {
                    if (Util.Config.DEBUG)
                        Debug.Log("Bad word resource non-match test passed");
                }
                else
                {
                    Debug.LogError("Bad word resource non-match word test failed");
                    failCounter++;
                }

                //arabic match test
                if (BWFManager.Contains(arabicBadword, mask))
                {
                    if (Util.Config.DEBUG)
                        Debug.Log("Arabic bad word match test passed");
                }
                else
                {
                    Debug.LogError("Arabic bad word match failed");
                    failCounter++;
                }

                //global match test
                if (BWFManager.Contains(globalBadword, mask))
                {
                    if (Util.Config.DEBUG)
                        Debug.Log("Global bad word match test passed");
                }
                else
                {
                    Debug.LogError("Global bad word match failed");
                    failCounter++;
                }

                //name match test
                if (BWFManager.Contains(nameBadword, mask))
                {
                    if (Util.Config.DEBUG)
                        Debug.Log("Name match test passed");
                }
                else
                {
                    Debug.LogError("Name match failed");
                    failCounter++;
                }

                //emoji match test
                if (BWFManager.Contains(emoji, mask))
                {
                    if (Util.Config.DEBUG)
                        Debug.Log("Emoji match test passed");
                }
                else
                {
                    Debug.LogError("Emoji match failed");
                    failCounter++;
                }
            }

            if ((mask & Model.ManagerMask.Domain) == Model.ManagerMask.Domain || (mask & Model.ManagerMask.All) == Model.ManagerMask.All)
            {
                //domain match test
                if (BWFManager.Contains(domain, mask))
                {
                    if (Util.Config.DEBUG)
                        Debug.Log("Domain match test passed");
                }
                else
                {
                    Debug.LogError("Domain match failed");
                    failCounter++;
                }

                //domain match test
                if (BWFManager.Contains(email, mask))
                {
                    if (Util.Config.DEBUG)
                        Debug.Log("Email match test passed");
                }
                else
                {
                    Debug.LogError("Email match failed");
                    failCounter++;
                }

                //domain non-match test
                if (!BWFManager.Contains(noDomain, mask))
                {
                    if (Util.Config.DEBUG)
                        Debug.Log("Domain non-match test passed");
                }
                else
                {
                    Debug.LogError("Domain non-match word test failed");
                    failCounter++;
                }
            }

            if ((mask & Model.ManagerMask.Capitalization) == Model.ManagerMask.Capitalization)
            {
                string caps = new string('A', Crosstales.BWF.Manager.CapitalizationManager.CharacterNumber);
                string noCaps = new string('A', Crosstales.BWF.Manager.CapitalizationManager.CharacterNumber - 1);

                //capital match test
                if (BWFManager.Contains(caps, mask))
                {
                    if (Util.Config.DEBUG)
                        Debug.Log("Capital match test passed");
                }
                else
                {
                    Debug.LogError("Capital match failed");
                    failCounter++;
                }

                //capital non-match test
                if (!BWFManager.Contains(noCaps, mask))
                {
                    if (Util.Config.DEBUG)
                        Debug.Log("Capital non-match test passed");
                }
                else
                {
                    Debug.LogError("Capital non-match word test failed");
                    failCounter++;
                }
            }

            if ((mask & Model.ManagerMask.Punctuation) == Model.ManagerMask.Punctuation)
            {
                string punc = new string('!', Crosstales.BWF.Manager.PunctuationManager.CharacterNumber);
                string noPunc = new string('!', Crosstales.BWF.Manager.PunctuationManager.CharacterNumber - 1);

                //punctuation match test
                if (BWFManager.Contains(punc))
                {
                    if (Util.Config.DEBUG)
                        Debug.Log("Punctuation match test passed");
                }
                else
                {
                    Debug.LogError("Punctuation match failed");
                    failCounter++;
                }

                //punctuation non-match test
                if (!BWFManager.Contains(noPunc))
                {
                    if (Util.Config.DEBUG)
                        Debug.Log("Punctuation non-match test passed");
                }
                else
                {
                    Debug.LogError("Punctuation non-match word test failed");
                    failCounter++;
                }
            }
        }

        #endregion
    }
}
// © 2015-2017 crosstales LLC (https://www.crosstales.com)