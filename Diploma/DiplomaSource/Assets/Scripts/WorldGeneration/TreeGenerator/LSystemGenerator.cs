using System.Collections.Generic;
using UnityEngine;

class LSystemGenerator : ObjectMeshGenerator
{
    // Параметры системы Лиденмайера
    public string InitialString; // Начальная строка
    public Rule[] Rules; // Правило записи

    [Range(0, 5)]
    public int Depth = 2;
    public float Length = 0.5f;
    public float Angle = 30;
    public float LengthScale = 0.75f;
    //public float WidthScale = 0.75f;

    public float UpperWidth = 0.05f;

    private string _currString;
    private int _maxCount = 0;
    private float _widthStep;

    //private void Start()
    //{

    //    _verticesOrigins = new List<Vector3>();
    //    GenerateShape();
    //}

    private string CalculateFinalString(string str)
    {
        string final = str;

        for (int i = 0; i < Depth; i++) {
            foreach (Rule rule in Rules) {
                final = final.Replace(rule.Key, rule.Value);
            }
        }

        return final;
    }

    private void OnDrawGizmos()
    {
        if (_verticesOrigins == null)
            return;

        for (int i = 1; i < _verticesOrigins.Count; i++) {
            Gizmos.DrawLine(_verticesOrigins[i].position, _verticesOrigins[i].parent.position);
            //Gizmos.DrawLine(_verticesOrigins[i].position, _verticesOrigins[i].children.position);
            Gizmos.DrawSphere(_verticesOrigins[i].position, 0.02f);
        }
        Gizmos.color = Color.green;
        for (int i = 1; i < _vertices.Count; i++) {
            Gizmos.DrawLine(_vertices[i].position, _vertices[i - 1].position);
            Gizmos.DrawSphere(_vertices[i].position, 0.02f);
        }
    }

    protected override void GenerateShape()
    {
        _currString = CalculateFinalString(InitialString);
        _verticesOrigins.Add(new Origin(Vector3.zero, Width, this));

        GenerateBranch(0, Vector3.zero, Vector3.up, Length, _verticesOrigins[0].SetParent(new Origin(Vector3.down, 0, this)), Vector3.zero, 1);

        foreach(Origin origin in _verticesOrigins) {
            float angle;
            Vector3 axis;
            CalculateQuaternion(origin.parent.position, origin.position).ToAngleAxis(out angle, out axis);
            print(angle + " " + axis);

            origin.Rotate(axis, angle * 180 / Mathf.PI);
        }
    }

    private int GenerateBranch(int charID, Vector3 pos, Vector3 dir, float length, Origin origin, Vector3 angles, int currCount)
    {
        Quaternion rotation = Quaternion.Euler(0, 0, 0);

        for (int i = charID; i < _currString.Length; i++) {
            switch (_currString[i]) {
                case 'F':
                    currCount++;
                    dir = MoveVertex(dir, angles);
                    angles = Vector3.zero;
                    pos += dir * length;

                    _verticesOrigins.Add(new Origin(pos, 1, this));
                    origin = _verticesOrigins[_verticesOrigins.Count - 1].SetParent(origin);
                    break;
                case 'H':
                    currCount++;
                    dir = MoveVertex(dir, angles);
                    angles = Vector3.zero;
                    pos += dir * length / 2;

                    _verticesOrigins.Add(new Origin(pos, 1, this));
                    origin = _verticesOrigins[_verticesOrigins.Count - 1].SetParent(origin);
                    break;
                case '+':
                    angles += Vector3.up * Angle;
                    break;
                case '-':
                    angles -= Vector3.up * Angle;
                    break;
                case '&':
                    angles += Vector3.back * Angle;
                    break;
                case '^':
                    angles -= Vector3.back * Angle;
                    break;
                case '\\':
                    angles += Vector3.right * Angle;
                    break;
                case '/':
                    angles -= Vector3.right * Angle;
                    break;
                case '"':
                    length *= LengthScale;
                    break;
                case '[':
                    i = GenerateBranch(i + 1, pos, dir, length, origin, angles, currCount);
                    //origin.SetNext(origin);
                    break;
                case ']':
                    if (_maxCount < currCount)
                        _maxCount = currCount;
                    return i;
            }
        }

        if (_maxCount < currCount)
            _maxCount = currCount;

        _widthStep = (Width - UpperWidth) / _maxCount;

        ResizeBranch(_verticesOrigins[0], 0);

        return 0;
    }

    private void ResizeBranch(Origin origin, int inBranchCount)
    {
        origin.SetScale((Width - _widthStep * inBranchCount) / Width);
        inBranchCount++;

        if (origin.GetChildrenCount() == 0)
            return;

        foreach (Origin child in origin.GetChildren())
            ResizeBranch(child, inBranchCount);
    }

    private Vector3 MoveVertex(Vector3 dir, Vector3 angles)
    {
        dir = RotateVector(RotateVector(Vector3.up, angles.y, Vector3.right), angles.x, dir).normalized;
        dir = RotateVector(RotateVector(Vector3.up, angles.y, Vector3.forward), angles.z, dir).normalized;

        //dir = new RotationMatrix(Vector3.right, angles.x).Rotate(dir);
        //dir = new RotationMatrix(Vector3.up, angles.y).Rotate(dir);
        //dir = new RotationMatrix(Vector3.forward, angles.z).Rotate(dir);

        //return (dir + Random.insideUnitSphere * 0.1f).normalized;
        return dir;
    }

    //Расчёт кватерниона между вектором и плоскостью XOZ
    public Quaternion CalculateQuaternion(Vector3 start, Vector3 end)
    {
        Vector3 vector = end - start;

        float angle = Vector3.Angle(new Vector3(vector.x, 0, vector.z), vector);
        Vector3 axis = Vector3.Cross(new Vector3(vector.x, 0, vector.z), vector).normalized;

        return Quaternion.AngleAxis(angle, axis);
    }
}

[System.Serializable]
public struct Rule
{
    public string Key;
    public string Value;
    public float Chance;
}