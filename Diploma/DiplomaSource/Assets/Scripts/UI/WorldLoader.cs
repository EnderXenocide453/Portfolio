using UnityEngine;
using System.Collections;
using UnityEngine.SceneManagement;

public static class WorldLoader
{
    //Параметры генерации
    public static int Seed = 0;
    public static int MapRadius = 10;
    public static float ChunkRadius = 100;
    public static int SubdivisionLevel = 10;
    public static float HeightScale = 400;
    public static float OceanHeight = 0.3f;
    public static int ErosionIterations;
    public static float TreeRenderRadius = 500, LeavesRenderRadius = 200, ChunkRenderRadius = 2000;

    //Функция принятия значений из настроек
    public static void LoadWorld(int seed, SettingsController controller)
    {
        Seed = seed;
        MapRadius = (int)controller.WorldRadiusSlider.value;
        ChunkRadius = controller.ChunkRadiusSlider.value;
        OceanHeight = controller.OceanSlider.value;
        HeightScale = controller.HeightSlider.value;
        ErosionIterations = (int)controller.ErosionSlider.value * 1000;
        TreeRenderRadius = controller.TreeRenderSlider.value;
        LeavesRenderRadius = controller.LeavesRenderSlider.value;
        ChunkRenderRadius = controller.ChunkRenderSlider.value;
        SceneManager.LoadScene(1);
    }
}