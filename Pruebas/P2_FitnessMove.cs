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
            if(!espaldaErguida(esqueleto.Joints[JointType.Head], esqueleto.Joints[JointType.ShoulderCenter], esqueleto.Joints[JointType.HipCenter])
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
        /// Método que comprueba si el esqueleto tiene la espalda erguida: 
        /// </summary>
        /// <param name="Head">Cabeza del esqueleto </param>
        /// <param name="Neck">Cuello del esqueleto </param>
        /// <param name="Pelvis">Pelvis del esqueleto </param>
        private bool (Joint Head, Joint Neck, Joint Pelvis)
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
    }
}