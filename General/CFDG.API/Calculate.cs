namespace CFDG.API
{
    public class Calculate
    {
        public static Triangle GetTriangle(double distance, double angle)
        {
            return new Triangle(distance, angle);
        }
    }
}
