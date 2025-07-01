using System;
using System.Collections.Generic;
using System.Diagnostics;
using Eduard;

namespace Eduard.Security
{
    /// <summary>
    /// Represents the modular bivariate polynomials.
    /// </summary>
    //[DebuggerStepThrough]
    public sealed class ModularPolynomial
    {
        public Node head;

        /// <summary>
        /// Creates an bivariate polynomial using integer value of 0.
        /// </summary>
        public ModularPolynomial()
        { head = null; }

        /// <summary>
        /// Creates an bivariate polynomial using specified large signed integer value.
        /// </summary>
        /// <param name="val"></param>
        public ModularPolynomial(BigInteger val)
        {
            head = new Node();
            head.coeff = val;

            head.degx = 0;
            head.degy = 0;
        }

        /// <summary>
        /// Creates an bivariate polynomial using the specified values.
        /// </summary>
        /// <param name="coeff"></param>
        /// <param name="degx"></param>
        /// <param name="degy"></param>
        public ModularPolynomial(BigInteger coeff, int degx, int degy)
        {
            head = new Node();
            head.coeff = coeff;
            head.degx = degx;
            head.degy = degy;
        }

        /// <summary>
        /// Creates an bivariate polynomial using a specified modular univariate polynomial.
        /// </summary>
        /// <param name="poly"></param>
        public ModularPolynomial(Polynomial poly)
        {
            for (int i = poly.Degree; i >= 0; i--)
                AddTerm(poly.coeffs[i], i, 0);
        }

        /// <summary>
        /// Creates an bivariate polynomial using a specified modular bivariate polynomial.
        /// </summary>
        /// <param name="poly"></param>
        public ModularPolynomial(ModularPolynomial poly)
        {
            Node node = poly.head;

            Node head = new Node();
            head.coeff = node.coeff;
            head.degx = node.degx;
            head.degy = node.degy;

            this.head = head;
            node = node.next;

            while(node != null)
            {
                head.next = new Node();
                head = head.next;

                head.coeff = node.coeff;
                head.degx = node.degx;

                head.degy = node.degy;
                node = node.next;
            }
        }

        /// <summary>
        /// Sets the field of bivariate polynomials.
        /// </summary>
        /// <param name="field"></param>
        public static void SetField(BigInteger field)
        { Polynomial.SetField(field); }

        /// <summary>
        /// Returns the coefficient of bivariate polynomial with specified degrees.
        /// </summary>
        /// <param name="degx"></param>
        /// <param name="degy"></param>
        /// <returns></returns>
        public BigInteger GetCoeff(int degx, int degy)
        {
            Node head = this.head;

            while(head != null)
            {
                if (degx == head.degx && degy == head.degy)
                    return head.coeff;

                head = head.next;
            }

            return 0;
        }

        /// <summary>
        /// Differentiate the modular bivariate polynomial in X.
        /// </summary>
        /// <param name="poly"></param>
        /// <returns></returns>
        public static ModularPolynomial Diff_dx(ModularPolynomial poly)
        {
            ModularPolynomial result = new ModularPolynomial();
            Node head = poly.head;

            while(head != null)
            {
                BigInteger coeff = Polynomial.MulMod(head.coeff, head.degx);

                if (coeff != 0)
                    result.AddTerm(coeff, head.degx - 1, head.degy);

                head = head.next;
            }

            return result;
        }

        /// <summary>
        /// Differentiate the modular bivariate polynomial in Y.
        /// </summary>
        /// <param name="poly"></param>
        /// <returns></returns>
        public static ModularPolynomial Diff_dy(ModularPolynomial poly)
        {
            ModularPolynomial result = new ModularPolynomial();
            Node head = poly.head;

            while (head != null)
            {
                BigInteger coeff = Polynomial.MulMod(head.coeff, head.degy);

                if (coeff != 0)
                    result.AddTerm(coeff, head.degx, head.degy - 1);

                head = head.next;
            }

            return result;
        }

        /// <summary>
        /// Evaluates the bivariate polynomial in Y terms specified by an large signed integer value.
        /// </summary>
        /// <param name="y"></param>
        /// <returns></returns>
        public Polynomial F(BigInteger y)
        {
            Node head = this.head;
            int maxy = 0;

            while(head != null)
            {
                if (maxy < head.degy) maxy = head.degy;
                head = head.next;
            }

            if (this.head == null) return 0;
            BigInteger[] py = new BigInteger[maxy + 1];
            py[0] = 1;

            for (int i = 1; i < py.Length; i++)
                py[i] = Polynomial.MulMod(py[i - 1], y);

            head = this.head;
            BigInteger[] coeffs = new BigInteger[head.degx + 1];

            for (int j = 0; j < coeffs.Length; j++)
                coeffs[j] = 0;

            while(head != null)
            {
                coeffs[head.degx] = Polynomial.AddMod(coeffs[head.degx], Polynomial.MulMod(head.coeff, py[head.degy]));
                head = head.next;
            }

            List<BigInteger> list = new List<BigInteger>();
            list.AddRange(coeffs);

            list.Reverse();
            return new Polynomial(list.ToArray());
        }

        /// <summary>
        /// Evaluates the bivariate polynomial in XY terms using the specified large signed integers.
        /// </summary>
        /// <param name="x"></param>
        /// <param name="y"></param>
        /// <returns></returns>
        public BigInteger F(BigInteger x, BigInteger y)
        {
            Polynomial poly = F(y);
            return poly.Horner(x);
        }
        
        /// <summary>
        /// Adds two modular bivariate polynomials.
        /// </summary>
        /// <param name="left"></param>
        /// <param name="right"></param>
        /// <returns></returns>
        public static ModularPolynomial operator +(ModularPolynomial left, ModularPolynomial right)
        {
            ModularPolynomial result = new ModularPolynomial(left);

            Node node = right.head;

            while(node != null)
            {
                result.AddTerm(node.coeff, node.degx, node.degy);
                node = node.next;
            }

            return result;
        }

        /// <summary>
        /// Subtracts an specified modular bivariate polynomial from another bivariate polynomial.
        /// </summary>
        /// <param name="left"></param>
        /// <param name="right"></param>
        /// <returns></returns>
        public static ModularPolynomial operator -(ModularPolynomial left, ModularPolynomial right)
        {
            ModularPolynomial result = new ModularPolynomial(left);

            Node node = right.head;

            while (node != null)
            {
                result.AddTerm(-node.coeff, node.degx, node.degy);
                node = node.next;
            }

            return result;
        }

        /// <summary>
        /// Multiplies an modular bivariate polynomial with an large signed integer value.
        /// </summary>
        /// <param name="left"></param>
        /// <param name="right"></param>
        /// <returns></returns>
        public static ModularPolynomial operator *(BigInteger left, ModularPolynomial right)
        {
            ModularPolynomial result = new ModularPolynomial();
            Node node = right.head;

            while(node != null)
            {
                result.AddTerm(left * node.coeff, node.degx, node.degy);
                node = node.next;
            }

            return result;
        }

        /// <summary>
        /// Multiplies two modular bivariate polynomials.
        /// </summary>
        /// <param name="left"></param>
        /// <param name="right"></param>
        /// <returns></returns>
        public static ModularPolynomial operator *(ModularPolynomial left, ModularPolynomial right)
        {
            ModularPolynomial result = new ModularPolynomial();
            Node lnode = left.head;

            while(lnode != null)
            {
                Node rnode = right.head;

                while (rnode != null)
                {
                    result.AddTerm(lnode.coeff * rnode.coeff, lnode.degx + rnode.degx, lnode.degy + rnode.degy);
                    rnode = rnode.next;
                }

                lnode = lnode.next;
            }

            return result;
        }

        /// <summary>
        /// Divides an specified bivariate polynomial by another modular bivariate polynomial.
        /// </summary>
        /// <param name="left"></param>
        /// <param name="right"></param>
        /// <returns></returns>
        public static ModularPolynomial operator /(ModularPolynomial left, ModularPolynomial right)
        {
            if (DegreeX(left) < DegreeX(right) && DegreeY(left) < DegreeY(right))
                return new ModularPolynomial();

            if (left == right)
                return new ModularPolynomial(1);

            ModularPolynomial temp = new ModularPolynomial(left);
            ModularPolynomial quo = new ModularPolynomial();

            Node lptr = temp.head;
            Node rptr = right.head;

            BigInteger inv = rptr.coeff.Inverse(Polynomial.field);

            while (lptr != null && lptr.degx >= rptr.degx && lptr.degy >= rptr.degy)
            {
                BigInteger q = Polynomial.MulMod(lptr.coeff, inv);
                int degx = lptr.degx - rptr.degx;

                int degy = lptr.degy - rptr.degy;
                Node ptr = right.head;

                quo.AddTerm(q, degx, degy);
                ModularPolynomial term = new ModularPolynomial(q, degx, degy);
                ModularPolynomial diff = term * right;

                temp -= diff;
                lptr = temp.head;
            }

            return quo;
        }

        /// <summary>
        /// Returns the modular bivariate polynomial which represents remainder of division of two bivariate polynomials.
        /// </summary>
        /// <param name="left"></param>
        /// <param name="right"></param>
        /// <returns></returns>
        public static ModularPolynomial operator %(ModularPolynomial left, ModularPolynomial right)
        {
            if (DegreeX(left) < DegreeX(right) && DegreeY(left) < DegreeY(right)) return left;
            ModularPolynomial result = new ModularPolynomial(left);

            Node lptr = result.head;
            Node rptr = right.head;

            BigInteger inv = rptr.coeff.Inverse(Polynomial.field);

            while(lptr != null && lptr.degx >= rptr.degx && lptr.degy >= rptr.degy)
            {
                BigInteger q = Polynomial.MulMod(lptr.coeff, inv);
                int degx = lptr.degx - rptr.degx;

                int degy = lptr.degy - rptr.degy;
                Node ptr = right.head;

                ModularPolynomial term = new ModularPolynomial(q, degx, degy);
                ModularPolynomial diff = term * right;

                result -= diff;
                lptr = result.head;
            }

            return result;
        }

        /// <summary>
        /// Adds a new XY term in modular bivariate polynomial using specified values.
        /// </summary>
        /// <param name="coeff"></param>
        /// <param name="degx"></param>
        /// <param name="degy"></param>
        public void AddTerm(BigInteger coeff, int degx, int degy)
        {
            coeff = Polynomial.Reduce(coeff);

            if(head == null)
            {
                head = new Node();
                head.coeff = coeff;
                head.degx = degx;
                head.degy = degy;
                return;
            }

            Node root = head;
            Node daddy = root;
            Node temp = head;

            if(!Find(coeff, degx, degy))
            {
                if ((degx > root.degx) || (degx == root.degx && degy > root.degy))
                {
                    Node newNode = new Node();
                    newNode.coeff = coeff;
                    newNode.degx = degx;
                    newNode.degy = degy;
                    newNode.next = root;
                    head = newNode;
                    return;
                }

                while(root != null)
                {
                    if ((degx > root.degx) || (degx == root.degx && degy > root.degy))
                    {
                        Node aux = new Node();
                        aux.coeff = coeff;
                        aux.degx = degx;
                        aux.degy = degy;
                        aux.next = root;
                        daddy.next = aux;
                        head = temp;
                        return;
                    }

                    daddy = root;
                    root = root.next;
                }

                Node next = new Node();
                next.coeff = coeff;
                next.degx = degx;
                next.degy = degy;
                daddy.next = next;
            }
        }

        private bool Find(BigInteger coeff, int degx, int degy)
        {
            Node head = this.head;
            Node temp = null;
            Node node = head;

            if(head.degx == degx && head.degy == degy)
            {
                BigInteger sum = Polynomial.AddMod(head.coeff, coeff);

                if(sum == 0)
                {
                    temp = head.next;
                    this.head = temp;
                    return true;
                }
                else
                {
                    head.coeff = sum;
                    return true;
                }
            }

            temp = head;
            head = head.next;

            while(head != null)
            {
                if(head.degx == degx && head.degy == degy)
                {
                    BigInteger sum = Polynomial.AddMod(head.coeff, coeff);

                    if(sum == 0)
                    {
                        temp.next = head.next;
                        this.head = node;
                        return true;
                    }
                    else
                    {
                        head.coeff = sum;
                        this.head = node;
                        return true;
                    }
                }

                temp = head;
                head = head.next;
            }

            return false;
        }

        internal ModularPolynomial PowX(int degree)
        {
            ModularPolynomial poly = new ModularPolynomial(1, degree, 0);
            return poly;
        }

        internal ModularPolynomial PowY(int degree)
        {
            ModularPolynomial poly = new ModularPolynomial(1, 0, degree);
            return poly;
        }

        /// <summary>
        /// Returns the degree of modular bivariate polynomial in X.
        /// </summary>
        /// <param name="poly"></param>
        /// <returns></returns>
        public static int DegreeX(ModularPolynomial poly)
        {
            if (poly.head == null) return 0;
            return poly.head.degx;
        }

        /// <summary>
        /// Returns the degree of modular bivariate polynomial in Y.
        /// </summary>
        /// <param name="poly"></param>
        /// <returns></returns>
        public static int DegreeY(ModularPolynomial poly)
        {
            Node head = poly.head;
            int maxdeg = 0;

            if (head == null) return 0;

            while(head != null)
            {
                if (maxdeg < head.degy)
                    maxdeg = head.degy;

                head = head.next;
            }

            return maxdeg;
        }

        /// <summary>
        /// Returns the hash code of this instance.
        /// </summary>
        /// <returns></returns>
        public override int GetHashCode()
        {
            return head.GetHashCode();
        }

        /// <summary>
        /// Determines whether two modular bivariate polynomials are equals.
        /// </summary>
        /// <param name="other"></param>
        /// <returns></returns>
        public bool Equals(ModularPolynomial other)
        {
            ModularPolynomial diff = this - other;

            if (diff.IsZero) return true;
            else return false;
        }

        /// <summary>
        /// Determines whether the specified objects are the same instance.
        /// </summary>
        /// <param name="obj"></param>
        /// <returns></returns>
        public override bool Equals(object obj)
        {
            if (object.ReferenceEquals(this, obj)) return true;

            return this.Equals((ModularPolynomial)obj);
        }

        /// <summary>
        /// Determines whether two modular bivariate polynomials are equals.
        /// </summary>
        /// <param name="left"></param>
        /// <param name="right"></param>
        /// <returns></returns>
        public static bool operator ==(ModularPolynomial left, ModularPolynomial right)
        {
            return left.Equals(right);
        }

        /// <summary>
        /// Determines whether two specified modular bivariate polynomials are not equals.
        /// </summary>
        /// <param name="left"></param>
        /// <param name="right"></param>
        /// <returns></returns>
        public static bool operator !=(ModularPolynomial left, ModularPolynomial right)
        {
            return !left.Equals(right);
        }

        /// <summary>
        /// Gets an value that represents if this bivariate polynomial are equals with integer value of 0.
        /// </summary>
        public bool IsZero
        { get { return (DegreeX(this) == 0 && DegreeY(this) == 0 && this.GetCoeff(0, 0) == 0); } }

        /// <summary>
        /// Gets an value that represents if the bivariate polynomial are equals with integer value of 1.
        /// </summary>
        public bool IsOne
        { get { return (DegreeX(this) == 0 && DegreeY(this) == 0 && this.GetCoeff(0, 0) == 1); } }

        /// <summary>
        /// Creates an modular bivariate polynomial using an large signed integer value.
        /// </summary>
        /// <param name="val"></param>
        public static implicit operator ModularPolynomial(BigInteger val)
        {
            return new ModularPolynomial(val);
        }

        /// <summary>
        /// Creates an bivariate polynomial using an specified modular univariate polynomial.
        /// </summary>
        /// <param name="poly"></param>
        public static implicit operator ModularPolynomial(Polynomial poly)
        {
            return new ModularPolynomial(poly);
        }

        ~ModularPolynomial()
        {
            while(head != null)
            {
                Node temp = head.next;
                head = null;
                head = temp;
            }
        }
    }
}
