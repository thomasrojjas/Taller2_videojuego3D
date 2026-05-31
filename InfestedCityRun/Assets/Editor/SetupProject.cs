#if UNITY_EDITOR
using System.IO;
using UnityEngine;
using UnityEngine.UI;
using UnityEditor;
using UnityEditor.Animations;

/// <summary>
/// Automation editor script that programmatically initializes folders, creates materials,
/// extracts animations, constructs prefabs, and builds the full gameplay scene for Infested City Run.
/// This guarantees a 100% functional scene setup without requiring manual drag-and-drop.
/// </summary>
public class SetupProject : MonoBehaviour
{
    private const string AssetsPath = "Assets/InfestedCityRunAssets";
    
    [MenuItem("Infested City Run/Build Project Scene and Prefabs")]
    public static void RunSetup()
    {
        Debug.Log("Starting Infested City Run automated project setup...");

        // 0. Register tags in ProjectSettings programmatically
        RegisterTags();

        // 1. Create directory structures
        CreateFolders();

        // 2. Setup standard materials
        Material heroMat = CreateColorMaterial("HeroMat", new Color(0.15f, 0.6f, 0.2f, 1f)); // Dark green
        Material zombieMat = CreateColorMaterial("ZombieMat", new Color(0.5f, 0.5f, 0.3f, 1f)); // Sickly pale yellow-green
        Material coinMat = CreateGlossyMaterial("CoinMat", new Color(1f, 0.85f, 0.1f, 1f), 0.9f, 0.8f); // Gold metallic
        Material bulletMat = CreateEmissiveMaterial("BulletMat", new Color(1f, 0.6f, 0f, 1f)); // Glowing orange
        Material obstacleMat = CreateColorMaterial("ObstacleMat", new Color(0.7f, 0.4f, 0.2f, 1f)); // Brownish barricade
        Material roadMat = CreateColorMaterial("RoadMat", new Color(0.2f, 0.2f, 0.2f, 1f)); // Dark asphalt grey
        
        // Car color materials for visual variations
        Material carMat1 = CreateColorMaterial("CarMat1", new Color(0.1f, 0.3f, 0.7f, 1f)); // Blue
        Material carMat2 = CreateColorMaterial("CarMat2", new Color(0.8f, 0.1f, 0.1f, 1f)); // Red
        Material carMat3 = CreateColorMaterial("CarMat3", new Color(0.9f, 0.9f, 0.9f, 1f)); // White / Taxi

        // 3. Load embedded animations and create Animator Controllers
        AnimatorController playerAnimator = CreatePlayerAnimator();
        AnimatorController zombieAnimator = CreateZombieAnimator();

        // 4. Construct Prefabs
        GameObject projectilePrefab = CreateProjectilePrefab(bulletMat);
        GameObject coinPrefab = CreateCoinPrefab(coinMat);
        GameObject playerPrefab = CreatePlayerPrefab(heroMat, playerAnimator, projectilePrefab);
        GameObject zombiePrefab = CreateZombiePrefab(zombieMat, zombieAnimator);
        GameObject barricadePrefab = CreateBarricadePrefab(obstacleMat);
        
        GameObject car1 = CreateCarPrefab("CarPolice", "car_police.fbx", carMat1);
        GameObject car2 = CreateCarPrefab("CarSedan", "car_sedan.fbx", carMat2);
        GameObject car3 = CreateCarPrefab("CarTaxi", "car_taxi.fbx", carMat3);
        GameObject[] carPrefabs = new GameObject[] { car1, car2, car3 };

        GameObject platformPrefab = CreatePlatformPrefab(roadMat, coinPrefab, zombiePrefab, carPrefabs, barricadePrefab);

        // 5. Build and Configure Scene
        BuildGameplayScene(playerPrefab, platformPrefab);

        Debug.Log("Project setup successfully completed! All assets, prefabs, and the main scene are configured.");
    }

    private static void RegisterTags()
    {
        SerializedObject tagManager = new SerializedObject(AssetDatabase.LoadAllAssetsAtPath("ProjectSettings/TagManager.asset")[0]);
        SerializedProperty tagsProp = tagManager.FindProperty("tags");
        
        string[] requiredTags = new string[] { "Enemy", "Obstacle", "Coin", "Projectile" };
        foreach (string t in requiredTags)
        {
            bool exists = false;
            for (int i = 0; i < tagsProp.arraySize; i++)
            {
                if (tagsProp.GetArrayElementAtIndex(i).stringValue == t)
                {
                    exists = true;
                    break;
                }
            }
            if (!exists)
            {
                int newIndex = tagsProp.arraySize;
                tagsProp.InsertArrayElementAtIndex(newIndex);
                tagsProp.GetArrayElementAtIndex(newIndex).stringValue = t;
                Debug.Log("Registered tag: " + t);
            }
        }
        tagManager.ApplyModifiedProperties();
    }

    private static void CreateFolders()
    {
        string[] folders = new string[] { "Prefabs", "Materials", "Scenes", "Animations" };
        foreach (string f in folders)
        {
            if (!AssetDatabase.IsValidFolder("Assets/" + f))
            {
                AssetDatabase.CreateFolder("Assets", f);
            }
        }
    }

    private static Material CreateColorMaterial(string name, Color color)
    {
        string path = "Assets/Materials/" + name + ".mat";
        Material mat = AssetDatabase.LoadAssetAtPath<Material>(path);
        if (mat == null)
        {
            mat = new Material(Shader.Find("Standard"));
            mat.color = color;
            AssetDatabase.CreateAsset(mat, path);
        }
        return mat;
    }

    private static Material CreateGlossyMaterial(string name, Color color, float metallic, float smoothness)
    {
        string path = "Assets/Materials/" + name + ".mat";
        Material mat = AssetDatabase.LoadAssetAtPath<Material>(path);
        if (mat == null)
        {
            mat = new Material(Shader.Find("Standard"));
            mat.color = color;
            mat.SetFloat("_Metallic", metallic);
            mat.SetFloat("_Glossiness", smoothness);
            AssetDatabase.CreateAsset(mat, path);
        }
        return mat;
    }

    private static Material CreateEmissiveMaterial(string name, Color color)
    {
        string path = "Assets/Materials/" + name + ".mat";
        Material mat = AssetDatabase.LoadAssetAtPath<Material>(path);
        if (mat == null)
        {
            mat = new Material(Shader.Find("Standard"));
            mat.color = color;
            mat.EnableKeyword("_EMISSION");
            mat.SetColor("_EmissionColor", color * 2f); // Glow effect
            AssetDatabase.CreateAsset(mat, path);
        }
        return mat;
    }

    private static AnimationClip FindEmbeddedAnimation(string fbxPath)
    {
        Object[] assets = AssetDatabase.LoadAllAssetsAtPath(fbxPath);
        foreach (Object asset in assets)
        {
            if (asset is AnimationClip && !asset.name.StartsWith("__"))
            {
                return (AnimationClip)asset;
            }
        }
        return null;
    }

    private static AnimatorController CreatePlayerAnimator()
    {
        string path = "Assets/Animations/PlayerAnimator.controller";
        AnimatorController controller = AnimatorController.CreateAnimatorControllerAtPath(path);

        // Load run clip
        AnimationClip runClip = FindEmbeddedAnimation(AssetsPath + "/PersonajesAnimados/Character_Hero@Walk With Rifle (1).fbx");
        if (runClip != null)
        {
            // Set loopable run
            var runSettings = AnimationUtility.GetAnimationClipSettings(runClip);
            runSettings.loopTime = true;
            AnimationUtility.SetAnimationClipSettings(runClip, runSettings);

            AnimatorStateMachine rootStateMachine = controller.layers[0].stateMachine;
            
            // Add Run State
            AnimatorState runState = rootStateMachine.AddState("Run");
            runState.motion = runClip;
            rootStateMachine.defaultState = runState;
            
            // Add Speed Multiplier parameter to scale animation speed with player run speed
            controller.AddParameter("SpeedMultiplier", AnimatorControllerParameterType.Float);
            runState.speedParameter = "SpeedMultiplier";
            runState.speedParameterActive = true;

            // Add triggers for other states
            controller.AddParameter("JumpTrigger", AnimatorControllerParameterType.Trigger);
            controller.AddParameter("ShootTrigger", AnimatorControllerParameterType.Trigger);
            controller.AddParameter("DieTrigger", AnimatorControllerParameterType.Trigger);
            controller.AddParameter("IsGrounded", AnimatorControllerParameterType.Bool);
            
            // Simple Jump State
            AnimatorState jumpState = rootStateMachine.AddState("Jump");
            AnimatorState dieState = rootStateMachine.AddState("Die");

            runState.AddTransition(jumpState).AddCondition(AnimatorConditionMode.If, 0, "JumpTrigger");
            jumpState.AddTransition(runState).AddCondition(AnimatorConditionMode.If, 0, "IsGrounded");

            rootStateMachine.AddAnyStateTransition(dieState).AddCondition(AnimatorConditionMode.If, 0, "DieTrigger");
        }

        return controller;
    }

    private static AnimatorController CreateZombieAnimator()
    {
        string path = "Assets/Animations/ZombieAnimator.controller";
        AnimatorController controller = AnimatorController.CreateAnimatorControllerAtPath(path);

        // Load zombie run clip
        AnimationClip runClip = FindEmbeddedAnimation(AssetsPath + "/PersonajesAnimados/Character_Zombie@Injured Run.fbx");
        if (runClip != null)
        {
            var runSettings = AnimationUtility.GetAnimationClipSettings(runClip);
            runSettings.loopTime = true;
            AnimationUtility.SetAnimationClipSettings(runClip, runSettings);

            AnimatorStateMachine rootStateMachine = controller.layers[0].stateMachine;
            
            // Add Run State
            AnimatorState runState = rootStateMachine.AddState("Run");
            runState.motion = runClip;
            rootStateMachine.defaultState = runState;

            controller.AddParameter("SpeedMultiplier", AnimatorControllerParameterType.Float);
            runState.speedParameter = "SpeedMultiplier";
            runState.speedParameterActive = true;
        }

        return controller;
    }

    private static GameObject CreateProjectilePrefab(Material bulletMat)
    {
        GameObject obj = GameObject.CreatePrimitive(PrimitiveType.Capsule);
        obj.name = "Projectile";
        obj.transform.localScale = new Vector3(0.15f, 0.15f, 0.5f);
        obj.transform.rotation = Quaternion.Euler(90f, 0f, 0f);

        // Configure MeshRenderer
        MeshRenderer renderer = obj.GetComponent<MeshRenderer>();
        if (renderer != null) renderer.material = bulletMat;

        // Configure physics
        DestroyImmediate(obj.GetComponent<CapsuleCollider>()); // Use Trigger SphereCollider for bullets
        SphereCollider trigger = obj.AddComponent<SphereCollider>();
        trigger.isTrigger = true;
        trigger.radius = 0.5f;

        Rigidbody rb = obj.AddComponent<Rigidbody>();
        rb.useGravity = false;
        rb.isKinematic = true;

        obj.AddComponent<Projectile>();
        obj.tag = "Projectile";

        string path = "Assets/Prefabs/Projectile.prefab";
        GameObject prefab = PrefabUtility.SaveAsPrefabAsset(obj, path);
        DestroyImmediate(obj);
        return prefab;
    }

    private static GameObject CreateCoinPrefab(Material coinMat)
    {
        GameObject obj = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
        obj.name = "Coin";
        obj.transform.localScale = new Vector3(0.6f, 0.08f, 0.6f);
        obj.transform.rotation = Quaternion.Euler(90f, 0f, 0f);

        MeshRenderer renderer = obj.GetComponent<MeshRenderer>();
        if (renderer != null) renderer.material = coinMat;

        DestroyImmediate(obj.GetComponent<CapsuleCollider>());
        SphereCollider trigger = obj.AddComponent<SphereCollider>();
        trigger.isTrigger = true;
        trigger.radius = 0.8f;

        obj.AddComponent<Coin>();
        obj.tag = "Coin";

        string path = "Assets/Prefabs/Coin.prefab";
        GameObject prefab = PrefabUtility.SaveAsPrefabAsset(obj, path);
        DestroyImmediate(obj);
        return prefab;
    }

    private static GameObject CreatePlayerPrefab(Material mat, AnimatorController animController, GameObject projPrefab)
    {
        GameObject root = new GameObject("Player");
        root.tag = "Player";

        // Import the character mesh FBX
        string fbxPath = AssetsPath + "/ArmasyPersonajes/PersonajesyObstaculos/Character_Hero.fbx";
        GameObject heroFbx = AssetDatabase.LoadAssetAtPath<GameObject>(fbxPath);
        if (heroFbx != null)
        {
            GameObject child = Instantiate(heroFbx, root.transform);
            child.name = "CharacterModel";
            ApplyMaterialRecursively(child, mat);
        }

        // Configure Rigidbody and Collider
        Rigidbody rb = root.AddComponent<Rigidbody>();
        rb.useGravity = true;
        rb.constraints = RigidbodyConstraints.FreezeRotation;

        CapsuleCollider collider = root.AddComponent<CapsuleCollider>();
        collider.height = 1.8f;
        collider.radius = 0.4f;
        collider.center = new Vector3(0f, 0.9f, 0f);

        // Setup Player Controller script
        PlayerController pc = root.AddComponent<PlayerController>();
        pc.projectilePrefab = projPrefab;

        // Configure Animator
        Animator anim = root.GetComponentInChildren<Animator>();
        if (anim == null && root.GetComponent<Animator>() != null) anim = root.GetComponent<Animator>();
        if (anim != null)
        {
            anim.runtimeAnimatorController = animController;
        }

        string path = "Assets/Prefabs/Player.prefab";
        GameObject prefab = PrefabUtility.SaveAsPrefabAsset(root, path);
        DestroyImmediate(root);
        return prefab;
    }

    private static GameObject CreateZombiePrefab(Material mat, AnimatorController animController)
    {
        GameObject root = new GameObject("Zombie");
        root.tag = "Enemy";

        string fbxPath = AssetsPath + "/ArmasyPersonajes/PersonajesyObstaculos/Character_Zombie.fbx";
        GameObject zombieFbx = AssetDatabase.LoadAssetAtPath<GameObject>(fbxPath);
        if (zombieFbx != null)
        {
            GameObject child = Instantiate(zombieFbx, root.transform);
            child.name = "CharacterModel";
            ApplyMaterialRecursively(child, mat);
        }

        Rigidbody rb = root.AddComponent<Rigidbody>();
        rb.useGravity = false;
        rb.isKinematic = true;

        CapsuleCollider collider = root.AddComponent<CapsuleCollider>();
        collider.height = 4f; // Tall enough to prevent jumping over
        collider.radius = 0.5f;
        collider.center = new Vector3(0f, 2f, 0f);

        root.AddComponent<EnemyZombie>();

        Animator anim = root.GetComponentInChildren<Animator>();
        if (anim == null && root.GetComponent<Animator>() != null) anim = root.GetComponent<Animator>();
        if (anim != null)
        {
            anim.runtimeAnimatorController = animController;
        }

        string path = "Assets/Prefabs/Zombie.prefab";
        GameObject prefab = PrefabUtility.SaveAsPrefabAsset(root, path);
        DestroyImmediate(root);
        return prefab;
    }

    private static GameObject CreateBarricadePrefab(Material mat)
    {
        GameObject root = new GameObject("Barricade");
        root.tag = "Obstacle";

        string fbxPath = AssetsPath + "/ArmasyPersonajes/PersonajesyObstaculos/Barricade_03.fbx";
        GameObject fbx = AssetDatabase.LoadAssetAtPath<GameObject>(fbxPath);
        if (fbx != null)
        {
            GameObject child = Instantiate(fbx, root.transform);
            child.name = "BarricadeModel";
            ApplyMaterialRecursively(child, mat);
        }

        BoxCollider collider = root.AddComponent<BoxCollider>();
        collider.size = new Vector3(2.2f, 1.2f, 0.6f);
        collider.center = new Vector3(0f, 0.6f, 0f);

        VehicleObstacle vo = root.AddComponent<VehicleObstacle>();
        vo.isVehicle = false;

        string path = "Assets/Prefabs/Barricade.prefab";
        GameObject prefab = PrefabUtility.SaveAsPrefabAsset(root, path);
        DestroyImmediate(root);
        return prefab;
    }

    private static GameObject CreateCarPrefab(string name, string fbxName, Material mat)
    {
        GameObject root = new GameObject(name);
        root.tag = "Obstacle";

        string fbxPath = AssetsPath + "/AutosyCiudad/KayKit_City_Builder_Bits_1.0_FREE/Assets/fbx (unity)/" + fbxName;
        GameObject fbx = AssetDatabase.LoadAssetAtPath<GameObject>(fbxPath);
        if (fbx != null)
        {
            GameObject child = Instantiate(fbx, root.transform);
            child.name = "CarModel";
            ApplyMaterialRecursively(child, mat);
        }

        BoxCollider collider = root.AddComponent<BoxCollider>();
        collider.size = new Vector3(1.8f, 1.3f, 3.6f);
        collider.center = new Vector3(0f, 0.65f, 0f);

        VehicleObstacle vo = root.AddComponent<VehicleObstacle>();
        vo.isVehicle = true;

        string path = "Assets/Prefabs/" + name + ".prefab";
        GameObject prefab = PrefabUtility.SaveAsPrefabAsset(root, path);
        DestroyImmediate(root);
        return prefab;
    }

    private static GameObject CreatePlatformPrefab(Material roadMat, GameObject coin, GameObject zombie, GameObject[] cars, GameObject barricade)
    {
        GameObject root = new GameObject("PlatformPrefab");

        // Ground Mesh
        string roadFbxPath = AssetsPath + "/AutosyCiudad/KayKit_City_Builder_Bits_1.0_FREE/Assets/fbx (unity)/road_straight.fbx";
        GameObject roadFbx = AssetDatabase.LoadAssetAtPath<GameObject>(roadFbxPath);
        if (roadFbx != null)
        {
            // Instantiating 5 road segments aligned sequentially to make it 20 units long (each road is approx 4 units long)
            for (int i = 0; i < 5; i++)
            {
                GameObject roadPart = Instantiate(roadFbx, root.transform);
                roadPart.name = "Road_" + i;
                roadPart.transform.localPosition = new Vector3(0f, 0f, (i - 2) * 4f); // Center segment at Z = 0
                ApplyMaterialRecursively(roadPart, roadMat);
            }
        }

        // Add lateral scenery/decorations (Buildings)
        string bldPath1 = AssetsPath + "/AutosyCiudad/KayKit_City_Builder_Bits_1.0_FREE/Assets/fbx (unity)/building_A.fbx";
        string bldPath2 = AssetsPath + "/AutosyCiudad/KayKit_City_Builder_Bits_1.0_FREE/Assets/fbx (unity)/building_B.fbx";
        string bldPath3 = AssetsPath + "/AutosyCiudad/KayKit_City_Builder_Bits_1.0_FREE/Assets/fbx (unity)/building_C.fbx";
        
        GameObject bld1 = AssetDatabase.LoadAssetAtPath<GameObject>(bldPath1);
        GameObject bld2 = AssetDatabase.LoadAssetAtPath<GameObject>(bldPath2);
        GameObject bld3 = AssetDatabase.LoadAssetAtPath<GameObject>(bldPath3);

        Material buildMat1 = CreateColorMaterial("BuildingMat1", new Color(0.8f, 0.4f, 0.3f, 1f)); // Brick red
        Material buildMat2 = CreateColorMaterial("BuildingMat2", new Color(0.3f, 0.6f, 0.7f, 1f)); // Light blue
        Material buildMat3 = CreateColorMaterial("BuildingMat3", new Color(0.4f, 0.4f, 0.4f, 1f)); // Dark grey

        // Spawn left structures
        if (bld1 != null)
        {
            GameObject lBld1 = Instantiate(bld1, root.transform);
            lBld1.name = "LeftBuilding_1";
            lBld1.transform.localPosition = new Vector3(-6.5f, 0f, -5f);
            lBld1.transform.localRotation = Quaternion.Euler(0f, 90f, 0f);
            ApplyMaterialRecursively(lBld1, buildMat1);
        }
        if (bld2 != null)
        {
            GameObject lBld2 = Instantiate(bld2, root.transform);
            lBld2.name = "LeftBuilding_2";
            lBld2.transform.localPosition = new Vector3(-6.5f, 0f, 5f);
            lBld2.transform.localRotation = Quaternion.Euler(0f, 90f, 0f);
            ApplyMaterialRecursively(lBld2, buildMat2);
        }

        // Spawn right structures
        if (bld3 != null)
        {
            GameObject rBld1 = Instantiate(bld3, root.transform);
            rBld1.name = "RightBuilding_1";
            rBld1.transform.localPosition = new Vector3(6.5f, 0f, -5f);
            rBld1.transform.localRotation = Quaternion.Euler(0f, -90f, 0f);
            ApplyMaterialRecursively(rBld1, buildMat3);
        }
        if (bld1 != null)
        {
            GameObject rBld2 = Instantiate(bld1, root.transform);
            rBld2.name = "RightBuilding_2";
            rBld2.transform.localPosition = new Vector3(6.5f, 0f, 5f);
            rBld2.transform.localRotation = Quaternion.Euler(0f, -90f, 0f);
            ApplyMaterialRecursively(rBld2, buildMat1);
        }

        // Setup base collider
        BoxCollider roadCollider = root.AddComponent<BoxCollider>();
        roadCollider.size = new Vector3(10f, 0.1f, 20f); // 20 units long, 10 units wide
        roadCollider.center = new Vector3(0f, -0.05f, 0f);

        // Bind Platform script
        Platform plat = root.AddComponent<Platform>();
        plat.coinPrefab = coin;
        plat.zombiePrefab = zombie;
        plat.vehiclePrefabs = cars;
        plat.barricadePrefab = barricade;
        plat.laneXs = new float[] { -3f, 0f, 3f };
        plat.spawnZOffset = 2f; // Spawn slightly ahead in the middle of platform

        string path = "Assets/Prefabs/Platform.prefab";
        GameObject prefab = PrefabUtility.SaveAsPrefabAsset(root, path);
        DestroyImmediate(root);
        return prefab;
    }

    private static void BuildGameplayScene(GameObject playerPref, GameObject platPref)
    {
        // 1. Create a new empty scene
        var scene = UnityEditor.SceneManagement.EditorSceneManager.NewScene(UnityEditor.SceneManagement.NewSceneSetup.EmptyScene);

        // 2. Spawn player
        GameObject playerObj = (GameObject)PrefabUtility.InstantiatePrefab(playerPref);
        playerObj.transform.position = new Vector3(0f, 0f, 0f);

        // 3. Create Directional Light
        GameObject lightObj = new GameObject("Directional Light");
        Light light = lightObj.AddComponent<Light>();
        light.type = LightType.Directional;
        light.intensity = 1.3f;
        light.shadows = LightShadows.Soft;
        lightObj.transform.rotation = Quaternion.Euler(50f, -30f, 0f);

        // 4. Create Main Camera
        GameObject camObj = new GameObject("Main Camera");
        camObj.tag = "MainCamera";
        Camera camera = camObj.AddComponent<Camera>();
        camera.clearFlags = CameraClearFlags.SolidColor;
        camera.backgroundColor = new Color(0.12f, 0.1f, 0.15f, 1f); // Dark apocalyptic purple-indigo background
        
        CameraFollow follow = camObj.AddComponent<CameraFollow>();
        follow.target = playerObj.transform;
        follow.yOffset = 4.5f;
        follow.zOffset = -7.5f;
        follow.fixedX = 0f;
        follow.pitchAngle = 18f;

        // 5. Create Audio Manager
        GameObject audioMgrObj = new GameObject("AudioManager");
        audioMgrObj.AddComponent<AudioManager>();

        // 6. Create GameManager
        GameObject gmObj = new GameObject("GameManager");
        GameManager gm = gmObj.AddComponent<GameManager>();
        gm.playerTransform = playerObj.transform;

        // 7. Create Platform Generator
        GameObject genObj = new GameObject("PlatformGenerator");
        PlatformGenerator generator = genObj.AddComponent<PlatformGenerator>();
        generator.platformPrefabs = new GameObject[] { platPref };
        generator.playerTransform = playerObj.transform;
        generator.platformLength = 20f;
        generator.maxActivePlatforms = 15;
        generator.recycleSafetyDistance = 15f;

        // 8. Setup Canvas UI
        GameObject canvasObj = new GameObject("Canvas");
        Canvas canvas = canvasObj.AddComponent<Canvas>();
        canvas.renderMode = RenderMode.ScreenSpaceOverlay;
        canvasObj.AddComponent<CanvasScaler>().uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        canvasObj.GetComponent<CanvasScaler>().referenceResolution = new Vector2(1920, 1080);
        canvasObj.AddComponent<GraphicRaycaster>();

        GameObject eventSystem = new GameObject("EventSystem");
        eventSystem.AddComponent<UnityEngine.EventSystems.EventSystem>();
        eventSystem.AddComponent<UnityEngine.EventSystems.StandaloneInputModule>();

        // 9. Setup UIManager
        GameObject uiMgrObj = new GameObject("UIManager");
        UIManager ui = uiMgrObj.AddComponent<UIManager>();

        // 10. HUD elements
        Font defaultFont = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");

        // Score Text
        GameObject scoreTextObj = new GameObject("ScoreText");
        scoreTextObj.transform.SetParent(canvasObj.transform);
        Text scoreText = scoreTextObj.AddComponent<Text>();
        scoreText.font = defaultFont;
        scoreText.fontSize = 42;
        scoreText.fontStyle = FontStyle.Bold;
        scoreText.color = Color.white;
        scoreText.text = "PUNTUACIÓN: 00000000";
        
        RectTransform scoreRect = scoreTextObj.GetComponent<RectTransform>();
        scoreRect.anchorMin = new Vector2(0f, 1f);
        scoreRect.anchorMax = new Vector2(0f, 1f);
        scoreRect.pivot = new Vector2(0f, 1f);
        scoreRect.anchoredPosition = new Vector2(50f, -50f);
        scoreRect.sizeDelta = new Vector2(600f, 60f);

        // Coins Text
        GameObject coinsTextObj = new GameObject("CoinsText");
        coinsTextObj.transform.SetParent(canvasObj.transform);
        Text coinsText = coinsTextObj.AddComponent<Text>();
        coinsText.font = defaultFont;
        coinsText.fontSize = 42;
        coinsText.fontStyle = FontStyle.Bold;
        coinsText.color = new Color(1f, 0.85f, 0.1f, 1f); // Gold
        coinsText.text = "MONEDAS: 00";
        
        RectTransform coinsRect = coinsTextObj.GetComponent<RectTransform>();
        coinsRect.anchorMin = new Vector2(0f, 1f);
        coinsRect.anchorMax = new Vector2(0f, 1f);
        coinsRect.pivot = new Vector2(0f, 1f);
        coinsRect.anchoredPosition = new Vector2(50f, -120f);
        coinsRect.sizeDelta = new Vector2(600f, 60f);

        // 11. Defeat Panel
        GameObject defeatPanelObj = new GameObject("DefeatPanel");
        defeatPanelObj.transform.SetParent(canvasObj.transform);
        Image panelBg = defeatPanelObj.AddComponent<Image>();
        panelBg.color = new Color(0.08f, 0.08f, 0.12f, 0.85f); // Beautiful dark tint

        RectTransform panelRect = defeatPanelObj.GetComponent<RectTransform>();
        panelRect.anchorMin = Vector2.zero;
        panelRect.anchorMax = Vector2.one;
        panelRect.offsetMin = Vector2.zero;
        panelRect.offsetMax = Vector2.zero;

        // Defeat Title Text
        GameObject titleTextObj = new GameObject("DefeatTitle");
        titleTextObj.transform.SetParent(defeatPanelObj.transform);
        Text titleText = titleTextObj.AddComponent<Text>();
        titleText.font = defaultFont;
        titleText.fontSize = 72;
        titleText.fontStyle = FontStyle.Bold;
        titleText.color = new Color(0.9f, 0.15f, 0.15f, 1f); // Dark red
        titleText.text = "¡HAS SIDO INFECTADO!";
        titleText.alignment = TextAnchor.MiddleCenter;

        RectTransform titleRect = titleTextObj.GetComponent<RectTransform>();
        titleRect.anchorMin = new Vector2(0.5f, 0.7f);
        titleRect.anchorMax = new Vector2(0.5f, 0.7f);
        titleRect.pivot = new Vector2(0.5f, 0.5f);
        titleRect.anchoredPosition = Vector2.zero;
        titleRect.sizeDelta = new Vector2(1000f, 100f);

        // Final Score Text
        GameObject finalScoreObj = new GameObject("FinalScoreText");
        finalScoreObj.transform.SetParent(defeatPanelObj.transform);
        Text finalScoreText = finalScoreObj.AddComponent<Text>();
        finalScoreText.font = defaultFont;
        finalScoreText.fontSize = 38;
        finalScoreText.color = Color.white;
        finalScoreText.text = "Puntaje Final: 0";
        finalScoreText.alignment = TextAnchor.MiddleCenter;

        RectTransform finalScoreRect = finalScoreObj.GetComponent<RectTransform>();
        finalScoreRect.anchorMin = new Vector2(0.5f, 0.55f);
        finalScoreRect.anchorMax = new Vector2(0.5f, 0.55f);
        finalScoreRect.pivot = new Vector2(0.5f, 0.55f);
        finalScoreRect.anchoredPosition = Vector2.zero;
        finalScoreRect.sizeDelta = new Vector2(800f, 60f);

        // Final Coins Text
        GameObject finalCoinsObj = new GameObject("FinalCoinsText");
        finalCoinsObj.transform.SetParent(defeatPanelObj.transform);
        Text finalCoinsText = finalCoinsObj.AddComponent<Text>();
        finalCoinsText.font = defaultFont;
        finalCoinsText.fontSize = 38;
        finalCoinsText.color = new Color(1f, 0.85f, 0.1f, 1f);
        finalCoinsText.text = "Monedas Recolectadas: 0";
        finalCoinsText.alignment = TextAnchor.MiddleCenter;

        RectTransform finalCoinsRect = finalCoinsObj.GetComponent<RectTransform>();
        finalCoinsRect.anchorMin = new Vector2(0.5f, 0.48f);
        finalCoinsRect.anchorMax = new Vector2(0.5f, 0.48f);
        finalCoinsRect.pivot = new Vector2(0.5f, 0.48f);
        finalCoinsRect.anchoredPosition = Vector2.zero;
        finalCoinsRect.sizeDelta = new Vector2(800f, 60f);

        // Restart Button
        GameObject buttonObj = new GameObject("RestartButton");
        buttonObj.transform.SetParent(defeatPanelObj.transform);
        
        Image btnImg = buttonObj.AddComponent<Image>();
        btnImg.color = new Color(0.85f, 0.25f, 0.2f, 1f); // Vibrant red-orange button

        Button btn = buttonObj.AddComponent<Button>();
        buttonObj.AddComponent<ButtonHover>(); // Hover micro-animation script!

        RectTransform btnRect = buttonObj.GetComponent<RectTransform>();
        btnRect.anchorMin = new Vector2(0.5f, 0.32f);
        btnRect.anchorMax = new Vector2(0.5f, 0.32f);
        btnRect.pivot = new Vector2(0.5f, 0.5f);
        btnRect.anchoredPosition = Vector2.zero;
        btnRect.sizeDelta = new Vector2(380f, 85f);

        // Button text child
        GameObject btnTextObj = new GameObject("Text");
        btnTextObj.transform.SetParent(buttonObj.transform);
        Text btnText = btnTextObj.AddComponent<Text>();
        btnText.font = defaultFont;
        btnText.fontSize = 28;
        btnText.fontStyle = FontStyle.Bold;
        btnText.color = Color.white;
        btnText.text = "REINICIAR PARTIDA";
        btnText.alignment = TextAnchor.MiddleCenter;

        RectTransform btnTextRect = btnTextObj.GetComponent<RectTransform>();
        btnTextRect.anchorMin = Vector2.zero;
        btnTextRect.anchorMax = Vector2.one;
        btnTextRect.offsetMin = Vector2.zero;
        btnTextRect.offsetMax = Vector2.zero;

        // Assign UIManager references
        ui.scoreText = scoreText;
        ui.coinsText = coinsText;
        ui.defeatPanel = defeatPanelObj;
        ui.finalScoreText = finalScoreText;
        ui.finalCoinsText = finalCoinsText;
        ui.restartButton = btn;

        // Deactivate defeat panel by default so it doesn't cover the screen in editor
        defeatPanelObj.SetActive(false);

        // 12. Save scene to assets and add to build settings
        string scenePath = "Assets/Scenes/GameplayScene.unity";
        UnityEditor.SceneManagement.EditorSceneManager.SaveScene(scene, scenePath);
        
        // Update Build Settings to include our scene
        EditorBuildSettingsScene[] buildScenes = new EditorBuildSettingsScene[] {
            new EditorBuildSettingsScene(scenePath, true)
        };
        EditorBuildSettings.scenes = buildScenes;
    }

    private static void ApplyMaterialRecursively(GameObject obj, Material mat)
    {
        MeshRenderer renderer = obj.GetComponent<MeshRenderer>();
        if (renderer != null)
        {
            Material[] mats = new Material[renderer.sharedMaterials.Length];
            for (int i = 0; i < mats.Length; i++)
            {
                mats[i] = mat;
            }
            renderer.sharedMaterials = mats;
        }

        SkinnedMeshRenderer skinnedRenderer = obj.GetComponent<SkinnedMeshRenderer>();
        if (skinnedRenderer != null)
        {
            Material[] mats = new Material[skinnedRenderer.sharedMaterials.Length];
            for (int i = 0; i < mats.Length; i++)
            {
                mats[i] = mat;
            }
            skinnedRenderer.sharedMaterials = mats;
        }

        foreach (Transform child in obj.transform)
        {
            ApplyMaterialRecursively(child.gameObject, mat);
        }
    }
}
#endif
