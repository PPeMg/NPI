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
        private double tolerancia;

        public P2_FitnessMove(double tol = 0.05)
        {
            this.tolerancia = tol;
        }

        public double getTolerancia() 
        {
            return this.tolerancia; 
        }

        public void setTolerancia(double tol)
        {
            this.tolerancia = tol;
        }

        /************************ Métodos de Comprobación del esqueleto ************************/
        /// <summary>
        /// Método que comprueba si estamos en la posición básica, a saber:
        /// Los brazos extendidos y paralelos al suelo.
        /// El tronco erguido.
        /// Las piernas extendidas, rectas y separadas por un ángulo de unos 60º
        /// </summary>
        /// <param name="esqueleto">Contiene el esqueleto sobre el que se trabaja</param>
        public bool PosicionBasica(Skeleton esqueleto)
        {
            bool enPosicion;

            // Primero vamos a controlar la posición de la columna.
            if (!EspaldaErguida(esqueleto.Joints[JointType.Head], esqueleto.Joints[JointType.ShoulderCenter], esqueleto.Joints[JointType.Spine]))
                enPosicion = false;
            // Ahora vamos a ver los brazos:
            else if (!brazosEnCruz(esqueleto.Joints[JointType.HandLeft], esqueleto.Joints[JointType.ElbowLeft],
                                    esqueleto.Joints[JointType.HandRight], esqueleto.Joints[JointType.ElbowRight]))
                enPosicion = false;
            // Por último, comprobamos las piernas:
            else if (!piernasAbiertas(esqueleto.Joints[JointType.FootLeft], esqueleto.Joints[JointType.FootRight], esqueleto.Joints[JointType.HipCenter]))
                enPosicion = false;
            else
                enPosicion = true;

            return enPosicion;
        }

        /// <summary>
        /// Método que comprueba si el esqueleto tiene la espalda erguida: 
        /// </summary>
        /// <param name="Head">Cabeza del esqueleto </param>
        /// <param name="Neck">Cuello del esqueleto </param>
        /// <param name="Pelvis">Pelvis del esqueleto </param>
        private bool EspaldaErguida(Joint Head, Joint Neck, Joint Pelvis)
        {
            bool enPosicion;

            // Tenemos que comprobar que la espalda esté recta, lo que se traduce en que cada 
            // articulación de la espalda esté aproximadamente en la misma X y en la misma Z:
            double topeSuperior = Neck.Position.X * (1.0 + this.tolerancia);
            double topeInferior = Neck.Position.X * (1.0 - this.tolerancia);

            if (Head.Position.X > topeSuperior || Pelvis.Position.X > topeSuperior)
                enPosicion = false;
            else if (Head.Position.X < topeInferior || Pelvis.Position.X < topeInferior)
                enPosicion = false;
            else
            {
                topeSuperior = Neck.Position.Z * (1.0 + this.tolerancia);
                topeInferior = Neck.Position.Z * (1.0 - this.tolerancia);

                if (Neck.Position.Z > topeSuperior || Pelvis.Position.Z > topeSuperior)
                    enPosicion = false;
                else if (Neck.Position.Z < topeInferior || Pelvis.Position.Z < topeInferior)
                    enPosicion = false;
                else
                    enPosicion = true;
            }

            return enPosicion;
        }



        /// <summary>
        /// Método que comprueba si los brazos están extendidos y a la misma altura (en cruz).
        /// </summary>
        /// <param name="LeftHand">Mano izquierda del esqueleto </param>
        /// <param name="LeftElbow">Codo izquierdo del esqueleto </param>
        /// <param name="RightHand">Mano derecha del esqueleto </param>
        /// <param name="RightElbow">Codo derecho del esqueleto </param>
        private bool brazosEnCruz(Joint LeftHand, Joint LeftElbow, Joint RightHand, Joint RightElbow)
        {
            bool enPosicion;

            // Comprobamos que el brazo izquierdo tiene todas sus articulaciones aproximadamente a la 
            // misma altura (tienen la misma Y). Empezamos con la mano con respecto a la muñeca:
            double topeSuperior = LeftHand.Position.Y * (1.0 + this.tolerancia);
            double topeInferior = LeftHand.Position.Y * (1.0 - this.tolerancia);

            if (LeftElbow.Position.Y > topeSuperior)
                enPosicion = false;
            else if (LeftElbow.Position.Y < topeInferior)
                enPosicion = false;
            else
            {
                // Ahora realizamos las mismas operaciones con el otro brazo:
                topeSuperior = RightHand.Position.Y * (1.0 + this.tolerancia);
                topeInferior = RightHand.Position.Y * (1.0 - this.tolerancia);

                if (RightElbow.Position.Y > topeSuperior)
                    enPosicion = false;
                else if (RightElbow.Position.Y < topeInferior)
                    enPosicion = false;
                else
                    enPosicion = true;
            }

            return enPosicion;
        }

        /// <summary>
        /// Método que comprueba si las piernas están abiertas 60º asumiendo que las dos piernas son del mismo tamaño 
        /// y aplicando una propiedad de los triángulos equiláteros.
        /// </summary>
        /// <param name="LeftFoot">Pie izquierdo del esqueleto </param>
        /// <param name="RightFoot">Pie derecho del esqueleto </param>
        /// <param name="Hip">Cadera del esqueleto </param>
        private bool piernasAbiertas(Joint LeftFoot, Joint RightFoot, Joint Hip)
        {
            bool enPosicion;

            // Calculamos la distancia entre los dos pies. Debe ser igual a la distancia existente entre 
            // cada pie y la cadera.
            double x = (double)(LeftFoot.Position.X - RightFoot.Position.X);
            double y = (double)(LeftFoot.Position.Y - RightFoot.Position.Y);
            double z = (double)(LeftFoot.Position.Z - RightFoot.Position.Z);

            x = x * x;
            y = y * y;
            z = z * z;

            double distanciaPies = Math.Sqrt(x+y+z);

            // Ahora hacemos lo mismo con la distancia entre la cadera y una pierna, por ejemplo con la izquierda;
            x = (double)(LeftFoot.Position.X - Hip.Position.X);
            y = (double)(LeftFoot.Position.Y - Hip.Position.Y);
            z = (double)(LeftFoot.Position.Z - Hip.Position.Z);

            x = x * x;
            y = y * y;
            z = z * z;

            double tamPiernas = Math.Sqrt(x + y + z);

            // Comprobamos que sean iguales aproximadamente, aplicando la tolerancia:
            if ((tamPiernas * (1.0 + this.tolerancia)) == distanciaPies)
                enPosicion = false;
            else if ((tamPiernas * (1.0 - this.tolerancia)) == distanciaPies)
                enPosicion = false;
            else
                enPosicion = true;

            return enPosicion;
        }
    }
}