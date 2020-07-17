using System;

namespace SimpleDataPrediction
{
    public class SquareMatrix : MyMatrix
    {
        int rank;

        /// <summary>Construct an identity matrix</summary>
        public SquareMatrix(int rank) : base(rank, rank)
        {
            this.rank = rank;
            for (int i = 0; i < rank; i++) this.data[i, i] = 1;
        }

        /// <summary>Construct a square matrix by copying the data</summary>
        public SquareMatrix(double[,] data) : base(data)
        {
            if (data.GetLength(0) != data.GetLength(1))
            {
                throw new ArgumentException("Only square matrix supported");
            }
            this.rank = data.GetLength(0);
        }

        /// <summary>Use Laplace expansion to calculate the determinant of a square matrix</summary>
        public double Determinant()
        {
            if (this.rank == 1)
            {
                return data[0, 0];
            }
            if (this.rank == 2)
            {
                return data[0, 0] * data[1, 1] - data[1, 0] * data[0, 1];
            }

            double det = 0;
            for (int i = 0; i < this.rank; i++)
            {
                det += this.data[i, 0] * this.Cofactor(i, 0).Determinant() * (i % 2 == 0 ? 1 : -1);
            }
            return det;
        }

        /// <summary>Use Cramer's rule to recursively find the inverse of a square matrix</summary>
        public SquareMatrix Inverse()
        {
            double det = this.Determinant();
            if (Math.Abs(det) < 1e-8) throw new InvalidOperationException("Cannot inverse a singular matrix");

            SquareMatrix result = new SquareMatrix(this.rank);
            for (int i = 0; i < this.rank; i++)
            {
                for (int j = 0; j < this.rank; j++)
                {
                    result.data[j, i] = this.Cofactor(i, j).Determinant() * ((i + j) % 2 == 0 ? 1 : -1) / det;
                }
            }
            return result;
        }

        /// <summary>Get a Cofactor matrix </summary>
        public SquareMatrix Cofactor(int i, int j)
        {
            if (this.rank < 2) throw new InvalidOperationException("Rank is less than two");

            SquareMatrix result = new SquareMatrix(this.rank - 1);
            double[,] buf = result.data;
            for (int y = 0; y < this.rank; y++)
            {
                for (int x = 0; x < this.rank; x++)
                {
                    if (y != i && x != j)
                    {
                        buf[y - (y > i ? 1 : 0), x - (x > j ? 1 : 0)] = this.data[y, x];
                    }
                }
            }
            return result;
        }

        /// <summary> Create a square matrix from a general matrix</summary>
        public static SquareMatrix FromMatrix(MyMatrix m)
        {
            if (m == null || m.Cols != m.Rows) throw new ArgumentException("Not a valid square matrix");
            return new SquareMatrix(m.RawData);
        }
    }
}
