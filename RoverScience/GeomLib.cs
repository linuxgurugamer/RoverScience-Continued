using System;
using UnityEngine;

namespace RoverScience
{
    // Much of the coordinate work with latitude/longitude in this source is only functional with the work here:
    // http://www.movable-type.co.uk/scripts/latlong.html
    public static class GeomLib
    {
        public static double GetDistanceBetweenTwoPoints(CelestialBody cb, Coords _from, Coords _to)
        {
            double bodyRadius = cb.Radius;
            double dLat = (_to.latitude - _from.latitude).ToRadians();
            double dLon = (_to.longitude - _from.longitude).ToRadians();
            double lat1 = _from.latitude.ToRadians();
            double lat2 = _to.latitude.ToRadians();

            double a = Math.Sin(dLat / 2) * Math.Sin(dLat / 2) +
                Math.Sin(dLon / 2) * Math.Sin(dLon / 2) * Math.Cos(lat1) * Math.Cos(lat2);
            double c = 2 * Math.Atan2(Math.Sqrt(a), Math.Sqrt(1 - a));
            double d = bodyRadius * c;

            return Math.Round(d, 4);
        }

        public static double GetBearingFromCoords(Coords target, Coords location)
        {
            // Rover x,y position

            double dLat = (target.latitude - location.latitude).ToRadians();
            double dLon = (target.longitude - location.longitude).ToRadians();
            double lat1 = location.latitude.ToRadians();
            double lat2 = target.latitude.ToRadians();

            double y = Math.Sin(dLon) * Math.Cos(lat2);
            double x = Math.Cos(lat1) * Math.Sin(lat2) -
                Math.Sin(lat1) * Math.Cos(lat2) * Math.Cos(dLon);

            double bearing = Math.Atan2(y, x).ToDegrees();
            //bearing = (bearing + 180) % 360;

            //return bearing % 360;
            return (bearing + 360) % 360;
        }

        public static double GetRoverHeading(Vessel vessel)
        {
            //Vector3d coM = vessel.findLocalCenterOfMass();
            Vector3d coM = vessel.localCoM;
            Vector3d up = (coM - vessel.mainBody.position).normalized;
            Vector3d north = Vector3d.Exclude(up, (vessel.mainBody.position +
                (Vector3d)vessel.mainBody.transform.up * vessel.mainBody.Radius) - coM).normalized;

            Quaternion rotationSurface = Quaternion.LookRotation(north, up);
            Quaternion rotationVesselSurface = Quaternion.Inverse(Quaternion.Euler(90, 0, 0) * Quaternion.Inverse(vessel.GetTransform().rotation) * rotationSurface);
            return rotationVesselSurface.eulerAngles.y;
        }

    }
}
