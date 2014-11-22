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
            if (!EspaldaErguida(esqueleto.Joints[JointType.Head], esqueleto.Joints[JointType.ShoulderCenter], esqueleto.Joints[JointType.HipCenter]))
                enPosicion = false;
            // Ahora vamos a ver los brazos:
            else if (!brazosEnCruz(esqueleto.Joints[JointType.HandLeft], esqueleto.Joints[JointType.WristLeft], esqueleto.Joints[JointType.ElbowLeft], esqueleto.Joints[JointType.ShoulderLeft],
                esqueleto.Joints[JointType.HandRight], esqueleto.Joints[JointType.WristRight], esqueleto.Joints[JointType.ElbowRight], esqueleto.Joints[JointType.ShoulderRight]))
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
            double topeSuperior = Head.Position.X * (1.0 + this.tolerancia);
            double topeInferior = Head.Position.X * (1.0 - this.tolerancia);

            if (Neck.Position.X > topeSuperior)
                enPosicion = false;
            else if (Neck.Position.X < topeInferior)
                enPosicion = false;
            else
            {
                topeSuperior = Head.Position.Z * (1.0 + this.tolerancia);
                topeInferior = Head.Position.Z * (1.0 - this.tolerancia);

                if (Neck.Position.Z > topeSuperior)
                    enPosicion = false;
                else if (Neck.Position.Z < topeInferior)
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
        /// <param name="LeftWrist">Muñeca izquierda del esqueleto </param>
        /// <param name="LeftElbow">Codo izquierdo del esqueleto </param>
        /// <param name="LeftShoulder">Hombro izquierdo del esqueleto </param>
        /// <param name="RightHand">Mano derecha del esqueleto </param>
        /// <param name="RightWrist">Muñeca dercha del esqueleto </param>
        /// <param name="RightElbow">Codo derecho del esqueleto </param>
        /// <param name="RightShoulder">Hombro derecho del esqueleto </param>
        private bool brazosEnCruz(Joint LeftHand, Joint LeftWrist, Joint LeftElbow, Joint LeftShoulder, Joint RightHand, Joint RightWrist, Joint RightElbow, Joint RightShoulder)
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
        /**************************************************************************************************** /
        /// <summary>
        /// Método que comprueba si los brazos están extendidos y a la misma altura (en cruz).
        /// </summary>
        /// <param name="LeftHand">Mano izquierda del esqueleto </param>
        /// <param name="LeftWrist">Muñeca izquierda del esqueleto </param>
        /// <param name="LeftElbow">Codo izquierdo del esqueleto </param>
        /// <param name="LeftShoulder">Hombro izquierdo del esqueleto </param>
        /// <param name="RightHand">Mano derecha del esqueleto </param>
        /// <param name="RightWrist">Muñeca dercha del esqueleto </param>
        /// <param name="RightElbow">Codo derecho del esqueleto </param>
        /// <param name="RightShoulder">Hombro derecho del esqueleto </param>
        private bool brazosEnCruz(Joint LeftHand, Joint LeftWrist, Joint LeftElbow, Joint LeftShoulder, Joint RightHand, Joint RightWrist, Joint RightElbow, Joint RightShoulder)
        {
            bool enPosicion;

            // Comprobamos que el brazo izquierdo tiene todas sus articulaciones aproximadamente a la 
            // misma altura (tienen la misma Y). Empezamos con la mano con respecto a la muñeca:
            double topeSuperior = LeftHand.Position.Y * (1.0 + this.tolerancia);
            double topeInferior = LeftHand.Position.Y * (1.0 - this.tolerancia);

            if (LeftWrist.Position.Y > topeSuperior)
                enPosicion = false;
            else if (LeftWrist.Position.Y < topeInferior)
                enPosicion = false;
            else
            {
                // Ahora comprobamos el codo (que está en medio, lo que nos ahorra algunas asignaciones)
                // con respecto de la muñeca y el hombro.
                topeSuperior = LeftElbow.Position.Y * (1.0 + this.tolerancia);
                topeInferior = LeftElbow.Position.Y * (1.0 - this.tolerancia);

                if (LeftWrist.Position.Y > topeSuperior)
                    enPosicion = false;
                else if (LeftWrist.Position.Y < topeInferior)
                    enPosicion = false;
                else if (LeftShoulder.Position.Y > topeSuperior)
                    enPosicion = false;
                else if (LeftShoulder.Position.Y < topeInferior)
                    enPosicion = false;
                else
                {
                    // Ahora realizamos las mismas operaciones con el otro brazo:
                    topeSuperior = RightHand.Position.Y * (1.0 + this.tolerancia);
                    topeInferior = RightHand.Position.Y * (1.0 - this.tolerancia);

                    if (RightWrist.Position.Y > topeSuperior)
                        enPosicion = false;
                    else if (RightWrist.Position.Y < topeInferior)
                        enPosicion = false;
                    else
                    {
                        // Ahora comprobamos el codo (que está en medio, lo que nos ahorra algunas asignaciones)
                        // con respecto de la muñeca y el hombro.
                        topeSuperior = RightElbow.Position.Y * (1.0 + this.tolerancia);
                        topeInferior = RightElbow.Position.Y * (1.0 - this.tolerancia);

                        if (RightWrist.Position.Y > topeSuperior)
                            enPosicion = false;
                        else if (RightWrist.Position.Y < topeInferior)
                            enPosicion = false;
                        else if (RightShoulder.Position.Y > topeSuperior)
                            enPosicion = false;
                        else if (RightShoulder.Position.Y < topeInferior)
                            enPosicion = false;
                        else
                            enPosicion = true;
                    }
                }
            }

            return enPosicion;
        }
        
        /****************************************************************************************************/
        
    }
}