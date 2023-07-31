using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Calculatrix
{
    public class Matrix
    {
        private double[,] data;
        public double determinant { get; private set; } = 0;
        public bool isDetermFound { get; private set; } = false;


        public int N { get; private set; }
        public int M { get; private set; }

        public Matrix(int n, int m)
        {
            N = n;
            M = m;
            data = new double[n, m];
            for (int i = 0; i < n; i++)
                for (int j = 0; j < m; j++)
                    data[i, j] = 0;
        }

        public bool SetElement(int x, int y, double num)
        {
            isDetermFound = false;
            if (x < 0 || x > N || y < 0 || y > M)
                return false;

            data[x, y] = num;
            return true;
        }

        public double GetElement(int i, int j) => data[i, j];

        public void Resize(int n, int m)
        {
            double[,] tmp = new double[n, m];

            for (int i = 0; i < n; i++)
                for (int j = 0; j < m; j++)
                {
                    if (i < N && j < M)
                        tmp[i, j] = data[i, j];
                    else
                        tmp[i, j] = 0;
                }

            isDetermFound = false;
            N = n;
            M = m;
            data = tmp;
        }

        public void SetNumToAll(double num)
        {
            isDetermFound = false;
            for (int i = 0; i < N; i++)
                for (int j = 0; j < M; j++)
                    data[i, j] = num;
        }

        public void RemoveRow(int index)//Удаление строки
        {
            int yMod;
            double[,] tmp = new double[N, M - 1];

            isDetermFound = false;
            for (int i = 0; i < N; i++) { 
                yMod = 0;
                for (int j = 0; j < M; j++) {
                    if (j == index) {
                        yMod = 1;
                        continue;
                    }
                    tmp[i, j - yMod] = data[i, j];
                }
            }

            data = tmp;
            M--;
        }

        public void RemoveColumn(int index)//Удаление колонки
        {
            int xMod = 0;
            double[,] tmp = new double[N - 1, M];

            isDetermFound = false;
            for (int i = 0; i < N; i++) {
                if (i == index) {
                    xMod = 1;
                    continue;
                }
                for (int j = 0; j < M; j++) {
                    tmp[i - xMod, j] = data[i, j];
                }
            }

            data = tmp;
            N--;
        }

        public void RemovePair(int x, int y) //Удаление строки и колонки в текущей матрице
        {
            RemoveColumn(x);
            RemoveRow(y);
        }

        public Matrix RemovedPair(int x, int y) //Удаление строки и колонки в копии текущей матрицы
        {
            Matrix matrix = new Matrix(N, M);
            matrix.data = data;

            matrix.RemoveRow(y);
            matrix.RemoveColumn(x);
            return matrix;
        }

        public bool IsSquare() //Проверка квадратности матрицы
        {
            return N == M;
        }

        public double Minor(int i, int j) => RemovedPair(i, j).Determinant(); //Нахождение минора матрицы

        public double Determinant() //Вычисление определителя
        {
            double determ = 0;
            
            if (!IsSquare()) {
                throw new NotSqareException();
            }
            
            if(N == 2)
                determ = data[0, 0] * data[1, 1] - data[1, 0] * data[0, 1];
            else
                for (int i = 0; i < N; i++) {
                    determ += (int)(data[0, i] * Math.Pow(-1, i) * Minor(0, i));
                }

            determinant = determ;
            isDetermFound = true;
            return determ;
        }

        public bool EqualsRange(Matrix B)
        {
            return N == B.N && M == B.M;
        }

        public Matrix MatrixSum(Matrix B)
        {
            Matrix answer = new Matrix(N, M);

            if (!EqualsRange(B))
                throw new NotEqualRangeException();

            for (int i = 0; i < N; i++)
                for (int j = 0; j < M; j++)
                {
                    answer.SetElement(i, j, data[i, j] + B.data[i, j]);
                }

            return answer;
        }

        public Matrix MatrixMult(Matrix B)
        {
            Matrix C = new Matrix(B.N, M);

            if (N != B.M)
                throw new MultRangeException();

            for (int i = 0; i < M; ++i)
            {
                for (int j = 0; j < B.N; ++j)
                {
                    C.data[i, j] = 0;
                    for (int k = 0; k < N; ++k)
                        C.data[i, j] += data[i, k] * B.data[k, j];
                }
            }

            return C;
        }

        public Matrix Transpose()
        {
            Matrix temp = new Matrix(M, N);

            for (int i = 0; i < N; i++)
                for (int j = 0; j < M; j++)
                    temp.SetElement(j, i, data[i, j]);

            return temp;
        }

        public Matrix ScalarMult(double num)
        {
            Matrix temp = new Matrix(N, M);
            temp.data = data;

            for (int i = 0; i < N; i++) {
                for (int j = 0; j < M; j++) {
                    temp.data[i, j] *= num;
                }
            }

            return temp;
        }

        public Matrix AlgebraicAdditionMatrix()
        {
            Matrix temp = new Matrix(N, M);
            int modifier = 1;

            for (int i = 0; i < N; i++)
                for (int j = 0; j < M; j++) {
                    temp.SetElement(i, j, Minor(i, j) * modifier);
                    modifier *= -1;
                }

            return temp;
        }

        public Matrix Inverse()
        {
            Matrix temp = new Matrix(N, M);

            if (!isDetermFound) Determinant();
            if (determinant == 0)
                return null;

            temp = AlgebraicAdditionMatrix().Transpose().ScalarMult(1 / determinant);

            return temp;
        }

        public void PrintMatrix()
        {
            for (int i = 0; i < N; i++)
            {
                for (int j = 0; j < M; j++)
                    Console.Write(data[i, j] + "\t");
                Console.WriteLine();
            }
        }
    }

    public class NotSqareException : Exception { }
    public class NotEqualRangeException : Exception { }
    public class MultRangeException : Exception { }
}
