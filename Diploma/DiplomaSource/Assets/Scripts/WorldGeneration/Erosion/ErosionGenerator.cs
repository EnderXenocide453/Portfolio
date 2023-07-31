using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;

//��������� ������ ����������� ����������� ��������� ������
public static class ErosionGenerator
{
    public static float brushRadius { get; private set; }

    private static List<Vector2Int> _brushIndices;
    private static List<float> _brushWeights;

    //������� ������
    public static void ErodeTerrain(HexMapGenerator generator, int iterations)
    {
        if (iterations == 0) return;
        GenerateBrushIndices(generator.ErosionRadius);

        VerticesContainer container = VerticesContainer.GetContainer();
        System.Random rand = new System.Random(generator.ErosionSeed); //������������� ���������� ��������������� �����
        float erodeRadius = generator.MapRadius * container.InnerRadius * 1.5f; //������ �����, � ������� ����� �������������� �����

        for (int iteration = 0; iteration < iterations; iteration++) {
            Vector3 global = VerticesContainer.GenerateRandomPointOnTerrain(rand, (container.MapRadius - 1) * container.SubdivisionLevel * 3 / 2);
            Vector2 pos = new Vector2(global.x, global.z) + new Vector2((float)rand.NextDouble() * 2 - 1, (float)rand.NextDouble() * 2 - 1) * container.ChunkRadius / container.SubdivisionLevel;
            Vector2 dir = Vector2.zero;
            float speed = generator.InitialSpeed;
            float volume = generator.InitialWaterVolume;
            float sediment = 0;

            for (int lifeTime = 0; lifeTime < generator.MaxDropletLifetime; lifeTime++) {
                //���������� ������� � ������
                SlopeAndHeight slope = CalculateSlopeAndHeight(pos);
                if (slope.error) break;

                //��������� ����������� � ������ ������� � �������
                dir = (dir * generator.ErosionInertia + slope.slope * (1 - generator.ErosionInertia)).normalized * speed;
                //��������� ��������� �����
                pos += dir;

                //������ � ������ � ����� �����
                SlopeAndHeight newSlope = CalculateSlopeAndHeight(pos);

                //���� ����� �� ��������� ������� ������ ��� ������������, �� ���� �������
                if (newSlope.error || dir == Vector2.zero) break;

                //���������� ������ � ����� ����� � ������� ����� ������ � ����� ��������
                float newHeight = newSlope.height;
                float deltaHeight = newHeight - slope.height;

                //���������� ���������� ������ ������ (��� ������ � ������� �����, ��� ������ ��� ����� ��������� ������)
                float sedimentCapasity = Mathf.Max(-deltaHeight * speed * volume * generator.SedimentCapacityFactor, generator.MinSedimentCapacity);

                //���� ������ ������ ��� ����� ����� ��������� ��� ����� ��������� �����
                if (sediment > sedimentCapasity || deltaHeight > 0) {
                    //������� ���������� ������, ������������ �� ���������� �������
                    //���� ����� �������� �����, ��� ���������� �������� ������, ����� ������ ����� ������ ����� ������
                    float deposit = deltaHeight > 0 ? Mathf.Min(deltaHeight, sediment) : (sediment - sedimentCapasity) * generator.DepositSpeed;
                    sediment -= deposit;

                    //���������� ��������� � ����� ������ �������
                    container.AddVertexHeight(slope.a, deposit * slope.coeff.x);
                    container.AddVertexHeight(slope.b, deposit * slope.coeff.y);
                    container.AddVertexHeight(slope.c, deposit * slope.coeff.z);
                } else {
                    //������� ���������� ������, ����������� �� ���������� �������
                    //����������� ���������� ��� �������������� ��� � ����������� �������
                    float erode = Mathf.Min(sedimentCapasity - sediment, -deltaHeight) * generator.ErodeSpeed;

                    //���� ������ � ����������� ��������� ���������� ����� ��������� �����
                    for (int i = 0; i < _brushIndices.Count; i++) {
                        float weighedErode = erode * _brushWeights[i];
                        //���� �������� ������ ������ ���������� ������, �� ��� ��������� ��������, ������ ������
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

    //������� ���������� ����� (������� �������� ������ �����, ���������� ��� ������� ������)
    private static void GenerateBrushIndices(float bRadius)
    {
        VerticesContainer container = VerticesContainer.GetContainer();

        //���� ������ ������ ����, �� �� ����������
        if (bRadius < 0) bRadius = 0;
        //���� ������ ������ ������� �����, �� �������������� ������� �����
        else if (bRadius > container.ChunkRadius * container.MapRadius) bRadius = container.InnerRadius * container.MapRadius;

        brushRadius = bRadius;

        _brushIndices = new List<Vector2Int>();
        _brushWeights = new List<float>();

        //���� ������ ����� ����, ������ ������������ ������ ����������� �������
        if (bRadius == 0) {
            _brushIndices.Add(Vector2Int.zero);
            _brushWeights.Add(1 / container.HeightScale);
            return;
        }

        //������ ��� ��������� ���������� ��������� ������
        int radius = (int)(bRadius / container.ChunkRadius * container.SubdivisionLevel);

        //��������� ������, ������� ������ ������� �������� ������, � ������. ������� ������� ������ �� ������� ��������� �������� ���������� �� ������� ���������� �� �������
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

    //������ ������� ����������� � � ������ � �������� �����
    public static SlopeAndHeight CalculateSlopeAndHeight(Vector2 pos)
    {
        VerticesContainer container = VerticesContainer.GetContainer();

        Vector2Int coord = VerticesContainer.GetVertexPos(new Vector3(pos.x, 0, pos.y)); //������ �������, ��������� � �������� �����
        if (!container.Vertices.ContainsKey(coord)) return new SlopeAndHeight() { error = true };

        //���������� ������� ����� �� ��������� �������
        Vector3 offset = VerticesContainer.WorldToCube(new Vector3(pos.x, 0, pos.y) - container.Vertices[coord][0].GlobalPos);

        Vector3 coeff; //������������ ��� ������� ������
        int sign;

        HexVertex a, b, c, origin = container.Vertices[coord][0];

        float currHeight = origin.Height;
        float slopeX = 0, slopeY = 0;

        //���������� ����� ������������
        //�������� ������ �� ������� �����
        try {
            //���� �������� � ������� X (����� �������� �� ������ ����)
            if (offset.y * offset.z > 0) {
                sign = System.Math.Sign(offset.x);
                a = origin;
                b = container.Vertices[VerticesContainer.HexToOddRowDisplace(origin.GlobalCubePos + new Vector3Int(sign, -sign, 0))][0];
                c = container.Vertices[VerticesContainer.HexToOddRowDisplace(origin.GlobalCubePos + new Vector3Int(sign, 0, -sign))][0];

                coeff = new Vector3(1 - offset.x * sign, offset.y * -sign, offset.z * -sign);
            }
            //����� � Y
            else if (offset.x * offset.z > 0) {
                sign = System.Math.Sign(offset.y);
                a = container.Vertices[VerticesContainer.HexToOddRowDisplace(origin.GlobalCubePos + new Vector3Int(-sign, sign, 0))][0];
                b = origin;
                c = container.Vertices[VerticesContainer.HexToOddRowDisplace(origin.GlobalCubePos + new Vector3Int(0, sign, -sign))][0];

                coeff = new Vector3(offset.x * -sign, 1 - offset.y * sign, offset.z * -sign);
            }
            //� Z
            else {
                sign = System.Math.Sign(offset.z);
                a = container.Vertices[VerticesContainer.HexToOddRowDisplace(origin.GlobalCubePos + new Vector3Int(-sign, 0, sign))][0];
                b = container.Vertices[VerticesContainer.HexToOddRowDisplace(origin.GlobalCubePos + new Vector3Int(0, -sign, sign))][0];
                c = origin;

                coeff = new Vector3(offset.x * -sign, offset.y * -sign, 1 - offset.z * sign);
            }
        } catch {
            //� ������ ������ ����������� ������� ������� � �������
            return new SlopeAndHeight() { error = true };
        }
        //��������� ������
        slopeX = (a.Height - b.Height) / container.InnerRadius * container.SubdivisionLevel * sign;
        slopeY = (c.Height - (a.Height + b.Height) / 2) / container.InnerCathet * -sign;

        //������� ������ �����
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

//����� ������� � ������
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
