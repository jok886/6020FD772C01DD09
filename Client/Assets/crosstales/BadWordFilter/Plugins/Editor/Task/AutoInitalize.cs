using UnityEditor;
using UnityEngine.SceneManagement;
using UnityEditor.SceneManagement;
using Crosstales.BWF.EditorUtil;

namespace Crosstales.BWF.EditorTask
{
    /// <summary>Automatically adds the neccessary BWF-prefabs to the current scene.</summary>
    [InitializeOnLoad]
    public class AutoInitalize
    {

        #region Variables

        private static Scene currentScene;

        #endregion


        #region Constructor

        static AutoInitalize()
        {
            EditorApplication.hierarchyWindowChanged += hierarchyWindowChanged;
        }

        #endregion


        #region Private static methods

        private static void hierarchyWindowChanged()
        {
            if (currentScene != EditorSceneManager.GetActiveScene())
            {
                if (EditorConfig.PREFAB_AUTOLOAD)
                {
                    if (!EditorHelper.isBWFInScene)
                        EditorHelper.InstantiatePrefab(Util.Constants.MANAGER_SCENE_OBJECT_NAME);
                }

                currentScene = EditorSceneManager.GetActiveScene();
            }
        }

        #endregion
    }
}
// Copyright 2016-2017 www.crosstales.com