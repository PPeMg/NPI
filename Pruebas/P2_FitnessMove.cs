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

        /// <summary>
        /// Valor que indica cuanto se tiene que mantener una postura para reconocerla:
        /// </summary>
        private int vecesNecesarias;

        /// <summary>
        /// Cadena de caracteres que se usa para almacenar el feedback del movimiento:
        /// </summary>
        private String feedBack;

        /************************ Fases del Movimiento y Control de duración ************************/
        /// <summary>
        /// Variable que se usa para comprobar si el esqueleto reconocido se ha situado en la posición base
        /// </summary>
        private bool posturaBaseFinalizada;

        /// <summary>
        /// Variable que se usa para comprobar la detección correcta de la postura un cierto número de veces:
        /// </summary>
        private int vecesCorrecta;


        /************************ Constructores y Métodos básicos de la clase ************************/
        public P2_FitnessMove(double tol = 0.05, int nveces = 10)
        {
            this.tolerancia = tol;
            this.vecesNecesarias = nveces;

            // Inicializamos todas las fases a falso, ya que aún no se ha ejecutado ninguna:
            this.posturaBaseFinalizada = false;
            this.vecesCorrecta = 0;
            
            this.feedBack = "";
        }

        public double getTolerancia() 
        {
            return this.tolerancia; 
        }

        public void setTolerancia(double tol)
        {
            this.tolerancia = tol;
        }

        public String getFeedBack()
        {
            return this.feedBack;
        }

        public void reiniciarMovimiento()
        {
            this.posturaBaseFinalizada = false;
            this.vecesCorrecta = 0;
        }

        /************************ Métodos de Comprobación del esqueleto ************************/
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
        /// <param name="RightWrist">Muñeca izquierda del esqueleto </param>
        /// <param name="LeftElbow">Codo izquierdo del esqueleto </param>
        /// <param name="RightWrist">Muñeca derecha del esqueleto </param>
        /// <param name="RightElbow">Codo derecho del esqueleto </param>
        private bool brazosEnCruz(Joint LeftWrist, Joint LeftElbow, Joint RightWrist, Joint RightElbow)
        {
            bool enPosicion;

            // Comprobamos que el brazo izquierdo tiene todas sus articulaciones aproximadamente a la 
            // misma altura (tienen la misma Y). Por cuestiones de precisión se usarán solo el codo y la
            // muñeca.
            double topeSuperior = LeftWrist.Position.Y * (1.0 + this.tolerancia);
            double topeInferior = LeftWrist.Position.Y * (1.0 - this.tolerancia);

            if (LeftElbow.Position.Y > topeSuperior)
                enPosicion = false;
            else if (LeftElbow.Position.Y < topeInferior)
                enPosicion = false;
            else
            {
                // Ahora realizamos las mismas operaciones con el otro brazo:
                topeSuperior = RightWrist.Position.Y * (1.0 + this.tolerancia);
                topeInferior = RightWrist.Position.Y * (1.0 - this.tolerancia);

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

            double distanciaPies = Math.Sqrt(x + y + z);

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
        
        /// <summary>
        /// Método que comprueba si estamos en la posición básica, a saber:
        /// Los brazos extendidos y paralelos al suelo.
        /// El tronco erguido.
        /// Las piernas extendidas, rectas y separadas por un ángulo de unos 60º
        /// *BASADO EN MOVIMIENTO 4 del GitHub: https://github.com/catiribeiro46/SkeletonBasics
        /// *En las pruebas realizadas no funcionaba correctamente, así que se cambio su implementación.
        /// </summary>
        /// <param name="esqueleto">Contiene el esqueleto sobre el que se trabaja</param>
        public bool PosicionBasica(Skeleton esqueleto)
        {
            bool enPosicion;

            // Primero vamos a controlar la posición de la columna.
            if (!EspaldaErguida(esqueleto.Joints[JointType.Head], esqueleto.Joints[JointType.ShoulderCenter], esqueleto.Joints[JointType.Spine]))
            {
                enPosicion = false;
                this.feedBack += "\tColóquese Recto.\n";
            }
            // Ahora comprobamos las piernas:
            else if (!piernasAbiertas(esqueleto.Joints[JointType.FootLeft], esqueleto.Joints[JointType.FootRight], esqueleto.Joints[JointType.HipCenter]))
            {
                enPosicion = false;
                this.feedBack += "\tColumna: OK.\n";
                this.feedBack += "\tAbra las piernas unos 60º\n";
            }
            // Por último, vamos a ver los brazos:
            else if (!brazosEnCruz(esqueleto.Joints[JointType.WristLeft], esqueleto.Joints[JointType.ElbowLeft],
                                    esqueleto.Joints[JointType.WristRight], esqueleto.Joints[JointType.ElbowRight]))
            {
                enPosicion = false;
                this.feedBack += "\tColumna: OK.\n";
                this.feedBack += "\tPiernas abiertas: OK\n";
                this.feedBack += "\tColoque los brazos en Cruz.\n";
            }
            
            // Si todo va bien, ponemos a verdadero el valor de retorno:
            else
            {
                enPosicion = true;
                this.feedBack += "\tColumna: OK.\n";
                this.feedBack += "\tPiernas abiertas: OK\n";
                this.feedBack += "\tBrazos en Cruz: OK\n";
            }

            return enPosicion;
        }

        /// <summary>
        /// Método que comprueba si la mano izquierda está sobre la cabeza del esqueleto. Para los cálculos
        /// se usará la muñeca izquierda, por detectarse con más precisión y estar muy cerca de la mano.
        /// *BASADO EN MOVIMIENTO 19 y 20 del github: https://github.com/Leontes/Kinect/
        /// *Se han observado también los github con los movimientos 21 y 22.
        /// *Modificado para adaptarlo a nuestro ejercicio concreto.
        /// </summary>
        /// <param name="RightWrist">Muñeca izquierda del esqueleto </param>
        /// <param name="Head">Cabeza del esqueleto </param>
        private bool ManoIzquierdaSobreCabeza(Joint LeftWrist, Joint Head)
        {
            bool enPosicion = false;

            // Comprobamos que la mano izquierda se encuentra sobre la cabeza. Para ello, debe cumplirse que la altura
            // de la muñeca sea algo superior a la de la cabeza (Coordenada Y) y que esté alineada en el eje X.
            double alturaASuperar = Head.Position.Y * (1.0 - this.tolerancia); // Tenemos que estar por encima de la cabeza
            // Cuanto más permisivos seamos, menos altura 
            // habrá que superar. Por tanto, la tolerancia
            // REDUCE la altura de la cabeza (Posicion Y).

            double XWristSup = LeftWrist.Position.X * (1.0 + this.tolerancia);
            double XWristInf = LeftWrist.Position.X * (1.0 - this.tolerancia);

            if (LeftWrist.Position.Y > alturaASuperar)
            {
                // Ahora la cabeza debe estar alineada con la muñeca en el eje X.
                if (Head.Position.X >= XWristInf && Head.Position.X <= XWristSup)
                    enPosicion = true;
            }

            return enPosicion;
        }

        /// <summary>
        /// Método que comprueba si la mano derecha está sobre la cabeza del esqueleto. Para los cálculos
        /// se usará la muñeca izquierda, por detectarse con más precisión y estar muy cerca de la mano.
        /// *BASADO EN MOVIMIENTO 19 y 20 del github: https://github.com/Leontes/Kinect/
        /// *Se han observado también los github con los movimientos 21 y 22.
        /// *Modificado para adaptarlo a nuestro ejercicio concreto.
        /// </summary>
        /// <param name="RightWrist">Muñeca derecha del esqueleto </param>
        /// <param name="Head">Cabeza del esqueleto </param>
        private bool ManoDerechaSobreCabeza(Joint RightWrist, Joint Head)
        {
            bool enPosicion = false;

            // Comprobamos que la mano dercha se encuentra sobre la cabeza. Para ello, debe cumplirse que la altura
            // de la muñeca sea algo superior a la de la cabeza (Coordenada Y) y que esté alineada en el eje X.
            double alturaASuperar = Head.Position.Y * (1.0 - this.tolerancia); // Tenemos que estar por encima de la cabeza
            // Cuanto más permisivos seamos, menos altura 
            // habrá que superar. Por tanto, la tolerancia
            // REDUCE la altura de la cabeza (Posicion Y).

            double XWristSup = RightWrist.Position.X * (1.0 + this.tolerancia);
            double XWristInf = RightWrist.Position.X * (1.0 - this.tolerancia);

            if (RightWrist.Position.Y > alturaASuperar)
            {
                // Ahora la cabeza debe estar alineada con la muñeca en el eje X.
                if (Head.Position.X >= XWristInf && Head.Position.X <= XWristSup)
                    enPosicion = true;
            }

            return enPosicion;
        }

        /// <summary>
        /// Método que comprueba la correcta realización del movimiento.
        /// </summary>
        public bool movimientoRealizado(Skeleton esqueleto)
        {
            this.feedBack = "Información del Movimiento: \n";

            // El retorno será false a menos que indiquemos lo contrario. Esto se hará solo cuando se haya terminado correctamente el movimiento:
            bool terminadoCorrectamente = false;

            //PRUEBAS:
            /**************************************************************************** /
            if (this.ManoIzquierdaSobreCabeza(esqueleto.Joints[JointType.WristLeft], esqueleto.Joints[JointType.Head]))
                terminadoCorrectamente = true;

            /****************************************************************************/
            if (this.ManoDerechaSobreCabeza(esqueleto.Joints[JointType.WristRight], esqueleto.Joints[JointType.Head]))
                terminadoCorrectamente = true;

            //DEFINITIVO:
            /**************************************************************************** /
            // Primero hay que controlar que partimos de la posición inicial:
            if (!posturaBaseFinalizada)
            {
                // Si no se ha establecido aún, comprobamos si está en la posición básica:
                if(this.PosicionBasica(esqueleto)){
                    this.vecesCorrecta ++;

                    // Si se ha mantenido el tiempo suficiente, ponemos a true la variable que comprueba que se ha 
                    // terminado correctamente esta fase:
                    if (this.vecesCorrecta >= this.vecesNecesarias)
                    {
                        posturaBaseFinalizada = true;
                        this.feedBack += "\nPosición Básica correcta.\n";
                    }
                    else
                        this.feedBack += "\nPosición Básica: " + this.vecesCorrecta + "\n";
                }

                // Si aún no hemos alcanzado la posición inicial, reiniciamos el acumulador:
                else
                    this.vecesCorrecta = 0;
            }


            else
            {
                terminadoCorrectamente = true;
            }

            /****************************************************************************/

            return terminadoCorrectamente;
        }
    }
}