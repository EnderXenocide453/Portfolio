using UnityEngine;
using UnityEngine.UI;

public class SettingsController : MonoBehaviour
{
    //Ссылки на слайдеры настроек
    public Slider WorldRadiusSlider,
        ChunkRadiusSlider,
        HeightSlider,
        OceanSlider,
        ErosionSlider,
        TreeRenderSlider,
        LeavesRenderSlider,
        ChunkRenderSlider;

    //При запуске сцены происходит привязка методов к событиям изменения слайдеров
    void Start()
    {
        WorldRadiusSlider.onValueChanged.AddListener(delegate { UpdateSliderValue(WorldRadiusSlider, GlobValue.MapRadius); });
        ChunkRadiusSlider.onValueChanged.AddListener(delegate { UpdateSliderValue(ChunkRadiusSlider, GlobValue.ChunkRadius); });
        HeightSlider.onValueChanged.AddListener(delegate { UpdateSliderValue(HeightSlider, GlobValue.Height); });
        OceanSlider.onValueChanged.AddListener(delegate { UpdateSliderValue(OceanSlider, GlobValue.Ocean); });
        ErosionSlider.onValueChanged.AddListener(delegate { UpdateSliderValue(ErosionSlider, GlobValue.Erosion); });
        TreeRenderSlider.onValueChanged.AddListener(delegate { UpdateSliderValue(TreeRenderSlider, GlobValue.Tree); });
        LeavesRenderSlider.onValueChanged.AddListener(delegate { UpdateSliderValue(LeavesRenderSlider, GlobValue.Leaves); });
        ChunkRenderSlider.onValueChanged.AddListener(delegate { UpdateSliderValue(ChunkRenderSlider, GlobValue.ChunkRender); });

        UpdateSliderValue(WorldRadiusSlider, GlobValue.MapRadius);
        UpdateSliderValue(ChunkRadiusSlider, GlobValue.ChunkRadius);
        UpdateSliderValue(HeightSlider, GlobValue.Height);
        UpdateSliderValue(OceanSlider, GlobValue.Ocean);
        UpdateSliderValue(ErosionSlider, GlobValue.Erosion);
        UpdateSliderValue(TreeRenderSlider, GlobValue.Tree);
        UpdateSliderValue(LeavesRenderSlider, GlobValue.Leaves);
        UpdateSliderValue(ChunkRenderSlider, GlobValue.ChunkRender);
    }

    //Метод изменения глобальных параметров соответственно значению 
    private void UpdateSliderValue(Slider slider, GlobValue valueName)
    {
        //Изменение значения по его имени
        switch (valueName) {
            case GlobValue.MapRadius: 
                WorldLoader.MapRadius = (int)slider.value;
                break;
            case GlobValue.ChunkRadius:
                WorldLoader.ChunkRadius = (int)slider.value;
                break;
            case GlobValue.Height:
                WorldLoader.HeightScale = (int)slider.value;
                break;
            case GlobValue.Ocean:
                WorldLoader.OceanHeight = (int)slider.value;
                break;
            case GlobValue.Erosion:
                WorldLoader.ErosionIterations = (int)slider.value;
                break;
            case GlobValue.Tree:
                WorldLoader.TreeRenderRadius = (int)slider.value;
                break;
            case GlobValue.Leaves:
                WorldLoader.LeavesRenderRadius = (int)slider.value;
                break;
            case GlobValue.ChunkRender:
                WorldLoader.ChunkRenderRadius = (int)slider.value;
                break;
        }
        
        //Изменение значения, отоброжаемого в окне настроек
        slider.transform.parent.GetChild(2).GetComponent<Text>().text = slider.value.ToString();
    }

    //Перечисление названий значений для их изменения в настройках
    private enum GlobValue
    {
        MapRadius,
        ChunkRadius,
        Height,
        Ocean,
        Erosion,
        Tree,
        Leaves,
        ChunkRender
    }
}
