using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SimpleDataPrediction
{
    /// <summary>
    /// https://bbs.csdn.net/topics/320245824
    /// </summary>
    public class GaussNewton
    {
        public delegate double F(double[] coefficients, double x);

        /// <summary> Construct a GaussNewton solver </summary>
        public GaussNewton(int coefficientCount)
        {
            this.coefficientCount = coefficientCount;
            this.coefficients = new double[coefficientCount];
        }

        /// <summary> Initialize the solver without a guess. Y = f(X) </summary>
        public void Initialize(double[] Y, double[] X, F f)
        {
            Initialize(Y, X, f, null);
        }

        /// <summary> Initialize the solver: Y = f(X) with a guessed approximation</summary>
        public void Initialize(double[] Y, double[] X, F f, double[] coefficientGuess)
        {
            if (X == null || Y == null || f == null) throw new ArgumentNullException();
            if (X.Length != Y.Length) throw new ArgumentException("Y and X not in pairs");

            this.xs = X;
            this.ys = Y;
            this.function = f;
            this.solved = false;

            if (coefficientGuess != null && coefficientGuess.Length == this.coefficientCount)
            {
                Array.Copy(coefficientGuess, this.coefficients, this.coefficientCount);
            }
        }

        /// <summary> Get the solved coefficents </summary>
        /// <exception cref="InvalidOperationException">Throw when not answer can be found</exception>
        public double[] Coefficients
        {
            get { if (!this.solved) { Solve(); } return this.coefficients; }
        }

        /// <summary> Get the residual error (SSE) </summary>
        public double SumOfSquredError
        {
            get { return this.sse; }
        }

        /// <summary> Iteratively solve the coefficients using Gauss-Newton method </summary>
        /// <remarks> http://en.wikipedia.org/wiki/Gauss%E2%80%93Newton_algorithm </remarks>
        public double[] Solve()
        {
            if (this.ys == null) throw new InvalidOperationException("Not yet initialized");

            double lastSSE = double.MaxValue;
            double[] errors = new double[this.ys.Length];

            // let C0 be the current coefficient guess, C1 be the better answer we are after
            // let E0 be the error using current guess
            // we have:
            // JacT * Jac * (C1 - C0) = JacT * E0
            //
            MyMatrix jacobian = Jacobian();
            MyMatrix jacobianT = jacobian.Transpose();
            MyMatrix product = jacobianT * jacobian;
            SquareMatrix inverse = SquareMatrix.FromMatrix(product).Inverse();

            for (int iteration = 0; iteration < GaussNewton.MaxIteration; iteration++)
            {
                this.sse = 0;

                for (int i = 0; i < this.ys.Length; i++)
                {
                    double y = function(this.coefficients, this.xs[i]);
                    errors[i] = this.ys[i] - y;
                    sse += errors[i] * errors[i];
                }

                if (lastSSE - sse < GaussNewton.ConvergeThreshold)
                {
                    this.solved = true;
                    return this.coefficients;
                }

                double[] shift = inverse * (jacobianT * errors);

                for (int i = 0; i < this.coefficientCount; i++)
                {
                    this.coefficients[i] += shift[i];
                }

                lastSSE = sse;
            }
            throw new InvalidOperationException("No answer can be found");
        }

        /// <summary> Calculate a Jacobian matrix. </summary>
        /// <remarks> http://en.wikipedia.org/wiki/Jacobian_matrix_and_determinant </remarks>
        private MyMatrix Jacobian()
        {
            double[][] p = new double[this.coefficientCount][];
            for (int i = 0; i < p.Length; i++)
            {
                p[i] = new double[this.coefficientCount];
                p[i][i] = 1;
            }

            MyMatrix jacobian = new MyMatrix(this.ys.Length, this.coefficientCount);
            for (int i = 0; i < this.ys.Length; i++)
            {
                for (int j = 0; j < this.coefficientCount; j++)
                {
                    jacobian[i, j] = function(p[j], this.xs[i]);
                }
            }
            return jacobian;
        }

        private bool solved;
        private int coefficientCount;
        private double[] coefficients;
        private double[] xs;
        private double[] ys;
        private double sse;
        private F function;

        public static readonly int MaxIteration = 16;
        public static readonly double Epsilon = 1e-10;
        public static readonly double ConvergeThreshold = 1e-8;
    }
}
