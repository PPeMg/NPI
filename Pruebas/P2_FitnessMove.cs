using System;
using Microsoft.Kinect;

namespace Microsoft.Samples.Kinect.ColorBasics
{
    /// <summary>
    /// Clase que implementará todo el movimiento Fitness necesario para la Práctica 2.
    /// </summary>
    class P2_FitnessMove
    {
        /// <summary>
        /// Porcentaje de error admitido en el movimiento.
        /// </summary>
        private float tolerancia;


        /// <summary>
        /// Método que comprueba si estamos en la posición básica, a saber:
        /// Los brazos extendidos y paralelos al suelo.
        /// El tronco erguido.
        /// Las piernas extendidas, rectas y separadas por un ángulo de unos 60º
        /// </summary>
        /// <param name=""></param>
        private bool posicionBasica(Skeleton received)
        {
            foreach (Joint joint in received.Joints)
            {
                if (joint.TrackingState == JointTrackingState.Tracked)
                {//first verify if the body is alignet and arms are in a relaxed position


                    //{//here verify if the feet are together
                    //use the same strategy that was used in the previous case of the arms in a  relaxed position
                    double HipCenterPosX = received.Joints[JointType.HipCenter].Position.X;
                    double HipCenterPosY = received.Joints[JointType.HipCenter].Position.Y;
                    double HipCenterPosZ = received.Joints[JointType.HipCenter].Position.Z;

                    //if left ankle is very close to right ankle then verify the rest of the skeleton points
                    //if (received.Joints[JointType.AnkleLeft].Equals(received.Joints[JointType.AnkleRight])) 
                    double AnkLPosX = received.Joints[JointType.AnkleLeft].Position.X;
                    double AnkLPosY = received.Joints[JointType.AnkleLeft].Position.Y;
                    double AnkLPosZ = received.Joints[JointType.AnkleLeft].Position.Z;

                    double AnkRPosX = received.Joints[JointType.AnkleRight].Position.X;
                    double AnkRPosY = received.Joints[JointType.AnkleRight].Position.Y;
                    double AnkRPosZ = received.Joints[JointType.AnkleRight].Position.Z;
                    //assume that the distance Y between HipCenter to each foot is the same
                    double distHiptoAnkleL = HipCenterPosY - AnkLPosY;
                    //caldulate admited error 5% that correspond to 9 degrees for each side
                    double radian1 = (4.5 * Math.PI) / 180;
                    double DistErrorL = distHiptoAnkleL * Math.Tan(radian1);
                    //determine of projected point from HIP CENTER to LEFT ANKLE and RIGHT and then assume error
                    double ProjectedPointFootLX = HipCenterPosX;
                    double ProjectedPointFootLY = AnkLPosY;
                    double ProjectedPointFootLZ = HipCenterPosZ;

                    double radian2 = (35 * Math.PI) / 180;
                    double DistSeparateFoot = distHiptoAnkleL * Math.Tan(radian2);
                    //DrawingVisual MyDrawingVisual = new DrawingVisual();


                    // could variate AnkLposX and AnkLPosY
                    if (Math.Abs(AnkRPosX - AnkLPosX) <= Math.Abs((DistSeparateFoot) + DistErrorL) && Math.Abs(AnkRPosX - AnkLPosX) >= Math.Abs((DistSeparateFoot) - DistErrorL))
                        return true;
                    else return false;


                }//CLOSE if (joint.TrackingState == JointTrackingState.Tracked)
            }
            // ESTO ES SOLO PARA QUE COMPILE SI HACE FALTA, ESTA MAL LA FUNCION COMPLETAMENTE.
            return true;
        }
    }
}