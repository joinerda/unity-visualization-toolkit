using System.Collections;
using System.Collections.Generic;
using System;

/// <summary>
/// Nebula test data
/// 
/// World Of Data: Copyright 2016-2018 David Joiner
/// </summary>
public class Nebula
{

	public double alpha = 0.5;
	public double beta   =2.0;
	public double gamma = 0.05;
	public double delta = 20.0;
	public double r0   = 0.5;
	public double phiD  = 10.0 / 180 * Math.PI;
	public double phiJ  = 70.0 / 180 * Math.PI;

	public double[,,] values;
	public double[,,] temps;
    public double[] x;
    public double[] y;
    public double[] z;
    int n = 128;
    double size = 10.0;
    double minDensity = 0.01;


    double[] linspace(double a, double b, int n)
    {
        double space = (b - a) / (n - 1);
        double[] array = new double[n];
        array[0] = a;
        array[n - 1] = b;
        for (int i = 1; i < n - 1; i++)
        {
            array[i] = a + i * space;
        }
        return array;
    }

	double getTemp(double x,double y, double z) {
		double r = System.Math.Sqrt (x * x + y * y + z * z);
		return getTemp (r);
	}

	double getTemp(double r) {
		if (r < r0)
			return 0.0;
		double temp = delta / (r * r);
		return temp;
	}

    double rho(double x, double y, double z)
    {
        double r = Math.Sqrt(x * x + y * y + z * z);
        double r_xz = Math.Sqrt(x * x + z * z);
        double phi = Math.Atan2(y, r_xz);
        return rho(r, phi);
    }

    double rho(double r, double phi)
    {
        double density = 0.0;
        if (r < r0)
            return 0.0;
        if (Math.Abs(phi) < phiD)
        {
            density += alpha / (r * r);
        }
        if (Math.Abs(phi) > phiJ)
        {
            density += beta / (r * r);
        }
		density += gamma / (r * r);
        return density;
    }
    // Use this for initialization
    public void CalculateDensity()
    {
		values = new double[n, n, n];
		temps = new double[n, n, n];
        x = linspace(-size / 2, size / 2, n);
        y = linspace(-size / 2, size / 2, n);
        z = linspace(-size / 2, size / 2, n);

        for (int i = 0; i < n; i++)
        {
            for (int j = 0; j < n; j++)
            {
                for (int k = 0; k < n; k++)
                {
                    values[i, j, k] = rho(x[i], y[j], z[k]);
					temps [i, j, k] = getTemp (x [i], y [j], z [k]);
                }
            }
        }
    }
}