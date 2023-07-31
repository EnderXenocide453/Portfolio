using System.Collections.Generic;
using UnityEngine;

public static class TreeModelGenerator
{
    //Функция генерации полигональной сетки вокруг формы дерева
    public static void GenerateTreeModel(TreeModel model)
    {
        //Рекурсивная функция генерации полигональной сетки ветви
        void GenerateBranchMesh(Branch branch, Vector3 startPos)
        {
            Vector3 endPos = startPos + branch.dir * branch.length * model.LengthScale;

            //Вычисление нормали
            Vector3 normal = Vector3.Cross(branch.dir, (branch.dir + Vector3.one).normalized).normalized;

            //Матрица поворота
            Matrix4x4 rotationMatrix = Matrix4x4.Rotate(Quaternion.AngleAxis(360 / model.RingSize, branch.dir));

            int count = model.Vertices.Count;

            for (int i = 0; i < model.RingSize; i++) {
                //Нижний треугольник
                model.Triangles.Add(count + i * 2);
                model.Triangles.Add(count + (i * 2 + 2) % (2 * model.RingSize));
                model.Triangles.Add(count + i * 2 + 1);
                //Верхний треугольник
                model.Triangles.Add(count + (i * 2 + 2) % (2 * model.RingSize));
                model.Triangles.Add(count + (i * 2 + 3) % (2 * model.RingSize));
                model.Triangles.Add(count + i * 2 + 1);

                //Размещение точек
                model.Vertices.Add(startPos + branch.radius * normal * model.RadiusScale);
                normal = rotationMatrix.MultiplyVector(normal);
                model.Vertices.Add(endPos + normal * model.RadiusScale * (branch.leaf ? branch.radius : branch.radius * Mathf.Max(model.Ratio, 1 - model.Ratio)));
            }

            //Управление листьями
            if (branch.leaf) {
                float radius = branch.branchFeed * model.LeavesScale;
                if (branch.leaves == null) {
                    branch.leaves = GameObject.Instantiate(model.LeavesObj, model.transform).GetComponent<LeavesModel>();
                    branch.leaves.GenerateLeaves(endPos + model.transform.position, Vector3.one * (radius > model.MinLeavesRadius ? radius : model.MinLeavesRadius) * model.SplitSize * model.LengthScale, 0, model.Hide);
                } else {
                    branch.leaves.SetPosition(endPos + model.transform.position);
                    branch.leaves.SetScale(Vector3.one * (radius > model.MinLeavesRadius ? radius : model.MinLeavesRadius) * model.SplitSize * model.LengthScale);
                }

                return;
            }

            if (branch.leaves != null)
                GameObject.Destroy(branch.leaves.gameObject);

            //Генерация плоских веток для отдаленного дерева
            rotationMatrix = Matrix4x4.Rotate(Quaternion.AngleAxis(180, branch.dir));
            count = model.FarVertices.Count;

            //Нижний треугольник
            model.FarTriangles.Add(count);
            model.FarTriangles.Add(count + 1);
            model.FarTriangles.Add(count + 2);
            //Верхний треугольник
            model.FarTriangles.Add(count + 1);
            model.FarTriangles.Add(count + 3);
            model.FarTriangles.Add(count + 2);

            //Размещение точек
            float endRadius = branch.leaf ? branch.radius : branch.radius * Mathf.Max(model.Ratio, 1 - model.Ratio);

            model.FarVertices.Add(startPos + branch.radius * normal * model.RadiusScale);
            model.FarVertices.Add(endPos + normal * model.RadiusScale * endRadius);
            normal = rotationMatrix.MultiplyVector(normal);
            model.FarVertices.Add(startPos + branch.radius * normal * model.RadiusScale);
            model.FarVertices.Add(endPos + normal * model.RadiusScale * endRadius);

            //Генерация дочерних веток
            GenerateBranchMesh(branch.a, endPos);
            GenerateBranchMesh(branch.b, endPos);
        }

        GenerateBranchMesh(model.root, Vector3.zero);
    }
}
