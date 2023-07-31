using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;

//симуляция эрозии посредством массообмена стекающих капель
public static class ErosionGenerator
{
    public static float brushRadius { get; private set; }

    private static List<Vector2Int> _brushIndices;
    private static List<float> _brushWeights;

    //Функция эрозии
    public static void ErodeTerrain(HexMapGenerator generator, int iterations)
    {
        if (iterations == 0) return;
        GenerateBrushIndices(generator.ErosionRadius);

        VerticesContainer container = VerticesContainer.GetContainer();
        System.Random rand = new System.Random(generator.ErosionSeed); //инициализация генератора псевдослучайных чисел
        float erodeRadius = generator.MapRadius * container.InnerRadius * 1.5f; //радиус круга, в котором будут генерироваться капли

        for (int iteration = 0; iteration < iterations; iteration++) {
            Vector3 global = VerticesContainer.GenerateRandomPointOnTerrain(rand, (container.MapRadius - 1) * container.SubdivisionLevel * 3 / 2);
            Vector2 pos = new Vector2(global.x, global.z) + new Vector2((float)rand.NextDouble() * 2 - 1, (float)rand.NextDouble() * 2 - 1) * container.ChunkRadius / container.SubdivisionLevel;
            Vector2 dir = Vector2.zero;
            float speed = generator.InitialSpeed;
            float volume = generator.InitialWaterVolume;
            float sediment = 0;

            for (int lifeTime = 0; lifeTime < generator.MaxDropletLifetime; lifeTime++) {
                //Нахождение наклона и высоты
                SlopeAndHeight slope = CalculateSlopeAndHeight(pos);
                if (slope.error) break;

                //Изменение направления с учётом наклона и инерции
                dir = (dir * generator.ErosionInertia + slope.slope * (1 - generator.ErosionInertia)).normalized * speed;
                //Изменение положения капли
                pos += dir;

                //Наклон и высота в новой точке
                SlopeAndHeight newSlope = CalculateSlopeAndHeight(pos);

                //Если капля за пределами области эрозии или остановилась, то цикл окончен
                if (newSlope.error || dir == Vector2.zero) break;

                //Нахождение высоты в новой точке и разницы между старой и новой высотами
                float newHeight = newSlope.height;
                float deltaHeight = newHeight - slope.height;

                //Вычисление вохможного объема осадка (чем больше и быстрее капля, тем больше она может перенести осадка)
                float sedimentCapasity = Mathf.Max(-deltaHeight * speed * volume * generator.SedimentCapacityFactor, generator.MinSedimentCapacity);

                //Если осадка больше чем капля может содержать или капля двигается вверх
                if (sediment > sedimentCapasity || deltaHeight > 0) {
                    //Рассчет количества осадка, оставляемого на предыдущей позиции
                    //Если капля движется вверх, она попытается сравнять высоту, иначе высоте будет отдана часть осадка
                    float deposit = deltaHeight > 0 ? Mathf.Min(deltaHeight, sediment) : (sediment - sedimentCapasity) * generator.DepositSpeed;
                    sediment -= deposit;

                    //Заполнение ближайших к точке вершин осадком
                    container.AddVertexHeight(slope.a, deposit * slope.coeff.x);
                    container.AddVertexHeight(slope.b, deposit * slope.coeff.y);
                    container.AddVertexHeight(slope.c, deposit * slope.coeff.z);
                } else {
                    //Рассчет количества осадка, забираемого из предыдущей позиции
                    //Ограничение количества для предотвращения дыр в поверхности рельефа
                    float erode = Mathf.Min(sedimentCapasity - sediment, -deltaHeight) * generator.ErodeSpeed;

                    //Сбор осадка с поверхности используя полученные ранее параметры кисти
                    for (int i = 0; i < _brushIndices.Count; i++) {
                        float weighedErode = erode * _brushWeights[i];
                        //Если значение эрозии больше оставшейся высоты, то она принимает значение, равное высоте
                        float deltaSediment = weighedErode > container.GetVertexHeight(_brushIndices[i]) ? container.GetVertexHeight(_brushIndices[i]) : weighedErode;
                        container.AddVertexHeight(_brushIndices[i] + slope.origin, -deltaSediment);
                        sediment += deltaSediment;
                    }
                }

                speed = Mathf.Sqrt(speed * speed + deltaHeight * generator.Gravity);
                volume *= 1 - generator.EvaporateSpeed;
            }
        }

        for(int i = 0; i < _brushIndices.Count; i++) {
            container.AddVertexHeight(_brushIndices[i], -_brushWeights[i]);
        }
    }

    //Рассчет параметров кисти (рассчёт индексов вокруг точки, попадающих под влияние эрозии)
    private static void GenerateBrushIndices(float bRadius)
    {
        VerticesContainer container = VerticesContainer.GetContainer();

        //Если радиус меньше нуля, то он обнуляется
        if (bRadius < 0) bRadius = 0;
        //Если радиус больше радиуса карты, он приравнивается радиусу карты
        else if (bRadius > container.ChunkRadius * container.MapRadius) bRadius = container.InnerRadius * container.MapRadius;

        brushRadius = bRadius;

        _brushIndices = new List<Vector2Int>();
        _brushWeights = new List<float>();

        //Если радиус равен нулю, эрозии подвергается только центральная вершина
        if (bRadius == 0) {
            _brushIndices.Add(Vector2Int.zero);
            _brushWeights.Add(1 / container.HeightScale);
            return;
        }

        //Радиус для получения кубических координат вершин
        int radius = (int)(bRadius / container.ChunkRadius * container.SubdivisionLevel);

        //Помещение вершин, лежащих внутри радиуса действия эрозии, в список. Степень влияния эрозии на вершину равняется квадрату расстояния от границы окружности до вершины
        int id = 0;
        for (int x = -radius; x <= radius; x++) {
            for (int y = -radius; y <= radius; y++) {
                int z = -(x + y);
                Vector3Int pos = new Vector3Int(x, y, z);

                if (!container.CubeToDisplaceCoords.ContainsKey(pos)) continue;

                HexVertex vertex = container.Vertices[container.CubeToDisplaceCoords[pos]][0];

                if (vertex.GlobalPos.magnitude > brushRadius) continue;

                _brushIndices.Add(vertex.GlobalDisplacePos);
                _brushWeights.Add((1 - Mathf.Pow(vertex.GlobalPos.magnitude / brushRadius, 2)) / container.HeightScale);

                id++;
            }
        }
    }

    //Расчёт наклона поверхности и её высоты в заданной точке
    public static SlopeAndHeight CalculateSlopeAndHeight(Vector2 pos)
    {
        VerticesContainer container = VerticesContainer.GetContainer();

        Vector2Int coord = VerticesContainer.GetVertexPos(new Vector3(pos.x, 0, pos.y)); //Индекс вершины, ближайшей к заданной точке
        if (!container.Vertices.ContainsKey(coord)) return new SlopeAndHeight() { error = true };

        //вычисление отступа точки от ближайшей вершины
        Vector3 offset = VerticesContainer.WorldToCube(new Vector3(pos.x, 0, pos.y) - container.Vertices[coord][0].GlobalPos);

        Vector3 coeff; //коэффициенты для расчёта высоты
        int sign;

        HexVertex a, b, c, origin = container.Vertices[coord][0];

        float currHeight = origin.Height;
        float slopeX = 0, slopeY = 0;

        //нахождение точек треугольника
        //Проверка выхода за пределы карты
        try {
            //если смещение в сторону X (имеет отличный от прочих знак)
            if (offset.y * offset.z > 0) {
                sign = System.Math.Sign(offset.x);
                a = origin;
                b = container.Vertices[VerticesContainer.HexToOddRowDisplace(origin.GlobalCubePos + new Vector3Int(sign, -sign, 0))][0];
                c = container.Vertices[VerticesContainer.HexToOddRowDisplace(origin.GlobalCubePos + new Vector3Int(sign, 0, -sign))][0];

                coeff = new Vector3(1 - offset.x * sign, offset.y * -sign, offset.z * -sign);
            }
            //ближе к Y
            else if (offset.x * offset.z > 0) {
                sign = System.Math.Sign(offset.y);
                a = container.Vertices[VerticesContainer.HexToOddRowDisplace(origin.GlobalCubePos + new Vector3Int(-sign, sign, 0))][0];
                b = origin;
                c = container.Vertices[VerticesContainer.HexToOddRowDisplace(origin.GlobalCubePos + new Vector3Int(0, sign, -sign))][0];

                coeff = new Vector3(offset.x * -sign, 1 - offset.y * sign, offset.z * -sign);
            }
            //к Z
            else {
                sign = System.Math.Sign(offset.z);
                a = container.Vertices[VerticesContainer.HexToOddRowDisplace(origin.GlobalCubePos + new Vector3Int(-sign, 0, sign))][0];
                b = container.Vertices[VerticesContainer.HexToOddRowDisplace(origin.GlobalCubePos + new Vector3Int(0, -sign, sign))][0];
                c = origin;

                coeff = new Vector3(offset.x * -sign, offset.y * -sign, 1 - offset.z * sign);
            }
        } catch {
            //В случае ошибки возвращение пустого объекта с ошибкой
            return new SlopeAndHeight() { error = true };
        }
        //получение уклона
        slopeX = (a.Height - b.Height) / container.InnerRadius * container.SubdivisionLevel * sign;
        slopeY = (c.Height - (a.Height + b.Height) / 2) / container.InnerCathet * -sign;

        //рассчет высоты точки
        currHeight = (a.Height * coeff.x + b.Height * coeff.y + c.Height * coeff.z) / (coeff.x + coeff.y + coeff.z);

        return new SlopeAndHeight()
        {
            height = currHeight,
            slope = new Vector2(slopeX, slopeY) * container.ChunkRadius,
            a = a.GlobalDisplacePos,
            b = b.GlobalDisplacePos,
            c = c.GlobalDisplacePos,
            origin = origin.GlobalDisplacePos,
            coeff = coeff,
            error = false
        };
    }
}

//Класс наклона и высоты
public class SlopeAndHeight
{
    public Vector2 slope;
    public float height;

    public Vector2Int a, b, c, origin;
    public Vector3 coeff;

    public bool error = true;

    public override string ToString()
    {
        return slope + " " + height;
    }
}
