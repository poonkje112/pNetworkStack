namespace pNetworkStack.Core.Data
{
	public class pVector
	{
		public float X, Y, Z;

		/// <summary>
		/// Converts an string to a pVector
		/// </summary>
		/// <param name="text">example: "X,Y,Z"</param>
		/// <returns>A new pVector</returns>
		public static pVector StringToPVector(string text)
		{
			pVector output = Zero();

			string[] values = text.Split(',');

			float.TryParse(values[0], out output.X);
			float.TryParse(values[1], out output.Y);
			float.TryParse(values[2], out output.Z);
			
			return output;
		}
		
		#region Constructors

		public pVector(float x, float y)
		{
			X = x;
			Y = y;
			Z = 0;
		}

		public pVector(float x, float y, float z)
		{
			X = x;
			Y = y;
			Z = z;
		}

		public static pVector Zero()
		{
			return new pVector(0, 0, 0);
		}

		public static pVector One()
		{
			return new pVector(1, 1, 1);
		}

		public static pVector Back()
		{
			return new pVector(0, 0, -1);
		}

		public static pVector Forward()
		{
			return new pVector(0, 0, 1);
		}

		public static pVector Down()
		{
			return new pVector(0, -1, 0);
		}

		public static pVector Up()
		{
			return new pVector(0, 1, 0);
		}

		public static pVector Left()
		{
			return new pVector(-1, 0, 0);
		}

		public static pVector Right()
		{
			return new pVector(1, 0, 0);
		}

		public static pVector NegativeInfinity()
		{
			return new pVector(float.NegativeInfinity, float.NegativeInfinity, float.NegativeInfinity);
		}

		public static pVector PositiveInfinity()
		{
			return new pVector(float.PositiveInfinity, float.PositiveInfinity, float.PositiveInfinity);
		}

		#endregion

		#region Overrides

		public override string ToString()
		{
			return $"{X},{Y},{Z}";
		}

		public override bool Equals(object obj)
		{
			if (ReferenceEquals(null, obj)) return false;
			if (ReferenceEquals(this, obj)) return true;
			if (obj.GetType() != GetType()) return false;
			return Equals((pVector) obj);
		}

		public bool Equals(pVector obj)
		{
			return (obj.X.Equals(X) && obj.Y.Equals(Y) && obj.Z.Equals(Z));
		}
		
		public override int GetHashCode()
		{
			unchecked
			{
				var hashCode = X.GetHashCode();
				hashCode = (hashCode * 397) ^ Y.GetHashCode();
				hashCode = (hashCode * 397) ^ Z.GetHashCode();
				return hashCode;
			}
		}

		#endregion

		#region Operators

		public static pVector operator +(pVector a, pVector b)
		{
			return new pVector(a.X + b.X, a.Y + b.Y, a.Z + b.Z);
		}

		public static pVector operator -(pVector a, pVector b)
		{
			return new pVector(a.X - b.X, a.Y - b.Y, a.Z - b.Z);
		}

		public static pVector operator *(pVector a, float value)
		{
			return new pVector(a.X * value, a.Y * value, a.Z * value);
		}

		public static pVector operator *(pVector a, int value)
		{
			return new pVector(a.X * value, a.Y * value, a.Z * value);
		}

		public static pVector operator /(pVector a, int value)
		{
			return new pVector(a.X / value, a.Y / value, a.Z / value);
		}

		public static bool operator ==(pVector a, pVector b)
		{
			if (a == null || b == null) return false;
			return (a.X.Equals(b.X) && a.Y.Equals(b.Y) && a.Z.Equals(b.Z));
		}

		public static bool operator !=(pVector a, pVector b)
		{
			if (a == null || b == null) return false;
			return (!a.X.Equals(b.X) && !a.Y.Equals(b.Y) && !a.Z.Equals(b.Z));
		}

		#endregion
	}
}