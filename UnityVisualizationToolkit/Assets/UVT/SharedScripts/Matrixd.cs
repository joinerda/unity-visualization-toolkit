using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Matrixd 
{
	int n;
	int m;
	double [] values;

	void setup(int n, int m, double [] values)
	{
		this.n = n;
		this.m = m;
		this.values = new double[values.Length];
		for(int i=0;i<values.Length;i++)
		{
			this.values[i] = values[i];
		}
	}

	public void set(int i, int j, double value)
	{
		values[i*m+j] = value;
	}

	public double get(int i, int j)
	{
		return values[i*m+j];
	}

	public int getN()
	{
		return n;
	}

	public int getM()
	{
		return m;
	}

	public double [] getValues()
	{
		return values;
	}

	public void setValues(double [] values)
	{
		this.values = new double[values.Length];
		for (int i = 0; i < values.Length; i++)
		{
			this.values[i] = values[i];
		}
	}

	public Matrixd(double [,]a)
	{
		n = a.GetLength(0);
		m = a.GetLength(1);
		values = new double[n*m];
		for(int i=0;i<n;i++)
		{
			for(int j=0;j<m;j++)
			{
				values[i*m+j] = a[i,j];
			}
		}
	}


	public Matrixd(Matrixd a)
	{
		setup(a.getN(),a.getM(),a.getValues());
	}

	public Matrixd()
	{
		setup(3,3,new double[9]);
	}

	public Matrixd(int n, int m)
	{
		setup(n,m,new double[n*m]);
	}

	public Matrixd(int n, int m, double [] values)
	{
		setup(n,m,values);
	}

	public static Matrixd Eyes(int n)
	{
		Matrixd retval = new Matrixd(n,n);
		for(int i=0;i<n;i++)
		{
			retval.set(i,i,1.0d);
		}
		return retval;
	}

	public static Matrixd Zeros(int n)
	{
		return new Matrixd(n,n);
	}

	public static Vector3 matmul(Vector3 b, Matrixd a)
	{
		int an = a.getN();
		if (an != 3)
		{
			Debug.Log("Matrix dimensions do not agree");
			return Vector3.zero;
		}
		double[] retvald = matmul(new double[] { b.x, b.y, b.z },a);
		Vector3 retval = new Vector3((float)retvald[0], (float)retvald[1], (float)retvald[2]);
		return retval;
	}

	public static Vector3 matmul(Matrixd a, Vector3 b)
	{
		int am = a.getM();
		if(am!=3)
		{
			Debug.Log("Matrix dimensions do not agree");
			return Vector3.zero;
		}
		double [] retvald = matmul(a,new double[] {b.x,b.y,b.z});
		Vector3 retval = new Vector3((float)retvald[0],(float)retvald[1],(float)retvald[2]);
		return retval;
	}

	public static Vector3d matmul(Vector3d b, Matrixd a)
	{
		int an = a.getN();
		if (an != 3)
		{
			Debug.Log("Matrix dimensions do not agree");
			return Vector3d.zero;
		}
		double[] retvald = matmul(new double[] { b.x, b.y, b.z },a);
		Vector3d retval = new Vector3d(retvald[0], retvald[1], retvald[2]);
		return retval;
	}

	public static Vector3d matmul(Matrixd a, Vector3d b)
	{
		int am = a.getM();
		if (am != 3)
		{
			Debug.Log("Matrix dimensions do not agree");
			return Vector3d.zero;
		}
		double[] retvald = matmul(a, new double[] { b.x, b.y, b.z });
		Vector3d retval = new Vector3d(retvald[0], retvald[1],retvald[2]);
		return retval;
	}

	public static double [] matmul(double []b, Matrixd a)
	{
		int an = a.getN();
		int am = a.getM();
		int bm = b.Length;
		int bn = 1;
		if (an != bm)
		{
			Debug.Log("Matrix dimensions do not agree");
			return null;
		}
		double[] retval = new double[bm];

		for (int i = 0; i < am; i++)
		{
			double sum = 0.0;
			for (int k = 0; k < an; k++)
			{
				sum += a.get(k, i) * b[k];
			}
			retval[i] = sum;
		}

		return retval;
	}

	public static double [] matmul(Matrixd a, double [] b)
	{
		int an = a.getN();
		int am = a.getM();
		int bn = b.Length;
		int bm = 1;
		if (am != bn)
		{
			Debug.Log("Matrix dimensions do not agree");
			return null;
		}
		double [] retval = new double[bn];

		for (int i = 0; i < an; i++)
		{
			double sum = 0.0;
			for (int k = 0; k < am; k++)
			{
				sum += a.get(i, k) * b[k];
			}
			retval[i]=sum;
		}

		return retval;
	}

	public static Matrixd matmul(Matrixd a, Matrixd b)
	{
		int an = a.getN();
		int am = a.getM();
		int bn = b.getN();
		int bm = b.getM();
		if(am!=bn)
		{
			Debug.Log("Matrix dimensions do not agree");
			return null;
		}
		Matrixd retval = new Matrixd(an,bm);

		for(int i=0;i<an;i++)
		{
			for(int j=0;j<bm;j++)
			{
				double sum=0.0;
				for(int k=0;k<am;k++)
				{
					sum += a.get(i,k)*b.get(k,j);
				}
				retval.set(i,j,sum);
			}
		}

		return retval;
	}

	public static Matrixd add(Matrixd a, Matrixd b)
	{
		int an = a.getN();
		int am = a.getM();
		int bn = b.getN();
		int bm = b.getM();
		if(an!=bn||am!=bm)
		{
			Debug.Log("Matrix dimensions do no agree");
			return null;
		}
		Matrixd retval = new Matrixd(an,am);
		for(int i=0;i<an;i++)
		{
			for(int j=0;j<am;j++)
			{
				retval.set(i,j,a.get(i,j)+b.get(i,j));
			}
		}
		return retval;
	}

	public static Matrixd operator +(Matrixd a, Matrixd b)
	{
		return Matrixd.add(a,b);
	}

	public void negate()
	{
		for(int i=0;i<n*m;i++)
		{
			values[i] = - values[i];
		}
	}

	public static Matrixd minus(Matrixd a)
	{
		Matrixd retval  = new Matrixd(a);
		retval.negate();
		return retval;
	}

	public static Matrixd minus(Matrixd a, Matrixd b)
	{
		Matrixd retval = a+Matrixd.minus(b);
		return retval;
	}

	public static Matrixd operator -(Matrixd a)
	{
		return Matrixd.minus(a);
	}

	public static Matrixd operator -(Matrixd a, Matrixd b)
	{
		return Matrixd.minus(a,b);
	}

	public static Matrixd matmul(Matrixd a, double b)
	{
		Matrixd retval = new Matrixd(a);
		double[] values = a.getValues();
		for (int i = 0; i < values.Length; i++)
		{
			values[i] *= b;
		}
		retval.setValues(values);
		return retval;
	}

	public static Matrixd matmul(double b, Matrixd a)
	{
		return Matrixd.matmul(a,b);
	}

	public static Vector3 operator *(Matrixd a, Vector3 b)
	{
		return Matrixd.matmul(a, b);
	}

	public static Vector3 operator *(Vector3 b, Matrixd a)
	{
		return Matrixd.matmul(b, a);
	}

	public static Vector3d operator *(Matrixd a, Vector3d b)
	{
		return Matrixd.matmul(a, b);
	}

	public static Vector3d operator *(Vector3d b, Matrixd a)
	{
		return Matrixd.matmul(b, a);
	}

	public static double [] operator *(Matrixd a, double [] b)
	{
		return Matrixd.matmul(a, b);
	}

	public static double [] operator *(double[] b, Matrixd a)
	{
		return Matrixd.matmul(b, a);
	}

	public static Matrixd operator *(Matrixd a, double b)
	{
		return Matrixd.matmul(a,b);
	}

	public static Matrixd operator /(Matrixd a, double b)
	{
		return Matrixd.matmul(a, 1.0/b);
	}

	public static Matrixd operator *(double b, Matrixd a)
	{
		return Matrixd.matmul(a, b);
	}

	// TODO : fix formatting
	public override string ToString()
	{
		string retval = "";
		for(int i=0;i<n*m;i++)
		{
			retval = string.Concat(retval,string.Format("{0} ",values[i]));
			if((i+1)%m==0) retval = string.Concat(retval, "\n");
		}
		return retval;
	}

}
