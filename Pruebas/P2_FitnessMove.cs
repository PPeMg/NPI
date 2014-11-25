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
        /// <param name="Shoulder">Cabeza del esqueleto </param>
        /// <param name="Elbow">Cuello del esqueleto </param>
        /// <param name="Wrist">Wrist del esqueleto </param>
        private bool EspaldaErguida(Joint Head, Joint Neck, Joint Pelvis)
        {
            bool enPosicion;

            // Tenemos que comprobar que la espalda esté recta, lo que se traduce en que cada 
            // articulación de la espalda esté aproximadamente en la misma X y en la misma Z:
            double topeSuperior;
            double topeInferior;

            if ((Neck.Position.X * (1.0 + this.tolerancia)) >= 0)
            {
                topeSuperior = Neck.Position.X * (1.0 + this.tolerancia);
                topeInferior = Neck.Position.X * (1.0 - this.tolerancia);
            }
            else
            {
                topeSuperior = Neck.Position.X * (1.0 - this.tolerancia);
                topeInferior = Neck.Position.X * (1.0 + this.tolerancia);
            }

            if (Head.Position.X > topeSuperior || Pelvis.Position.X > topeSuperior)
                enPosicion = false;
            else if (Head.Position.X < topeInferior || Pelvis.Position.X < topeInferior)
                enPosicion = false;
            else
            {
                if ((Neck.Position.Z * (1.0 + this.tolerancia)) >= 0)
                {
                    topeSuperior = Neck.Position.Z * (1.0 + this.tolerancia);
                    topeInferior = Neck.Position.Z * (1.0 - this.tolerancia);
                }
                else
                {
                    topeSuperior = Neck.Position.Z * (1.0 - this.tolerancia);
                    topeInferior = Neck.Position.Z * (1.0 + this.tolerancia);
                }   

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
            double topeSuperior;
            double topeInferior;

            if ((LeftWrist.Position.Y * (1.0 + this.tolerancia)) >= 0)
            {
                topeSuperior = LeftWrist.Position.Y * (1.0 + this.tolerancia);
                topeInferior = LeftWrist.Position.Y * (1.0 - this.tolerancia);
            }
            else
            {
                topeSuperior = LeftWrist.Position.Y * (1.0 - this.tolerancia);
                topeInferior = LeftWrist.Position.Y * (1.0 + this.tolerancia);
            }
            if (LeftElbow.Position.Y > topeSuperior)
                enPosicion = false;
            else if (LeftElbow.Position.Y < topeInferior)
                enPosicion = false;
            else
            {
                // Ahora realizamos las mismas operaciones con el otro brazo:
                if ((RightWrist.Position.Y * (1.0 + this.tolerancia)) >= 0)
                {
                    topeSuperior = RightWrist.Position.Y * (1.0 + this.tolerancia);
                    topeInferior = RightWrist.Position.Y * (1.0 - this.tolerancia);
                }
                else
                {
                    topeSuperior = RightWrist.Position.Y * (1.0 - this.tolerancia);
                    topeInferior = RightWrist.Position.Y * (1.0 + this.tolerancia);
                }

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
        /// Método que comprueba si las piernas están abiertas más de 30º y menos de 60º (lo que es más o menos
        /// cómodo para realizar los movimientos.) asumiendo que las dos piernas son del mismo tamaño y 
        /// aplicando una propiedad de los triángulos equiláteros.
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
            if (distanciaPies < (tamPiernas/2 * (1.0 - this.tolerancia)))
                enPosicion = false;
            else if (distanciaPies > (tamPiernas * (1.0 + this.tolerancia)))
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
        /// Método que comprueba si la mano indicada está sobre la cabeza del esqueleto. Para los cálculos
        /// se usará la muñeca izquierda, por detectarse con más precisión y estar muy cerca de la mano.
        /// *BASADO EN MOVIMIENTO 19 y 20 del github: https://github.com/Leontes/Kinect/
        /// *Se han observado también los github con los movimientos 21 y 22.
        /// *Modificado para adaptarlo a nuestro ejercicio concreto.
        /// </summary>
        /// <param name="Wrist">Muñeca izquierda del esqueleto </param>
        /// <param name="Head">Cabeza del esqueleto </param>
        private bool manoSobreCabeza(Joint Wrist, Joint Head)
        {
            bool enPosicion = false;

            // Comprobamos que la mano izquierda se encuentra sobre la cabeza. Para ello, debe cumplirse que la altura
            // de la muñeca sea algo superior a la de la cabeza (Coordenada Y) y que esté alineada en el eje X.
            double alturaASuperar;

            // Tenemos que estar por encima de la cabeza           
            // Cuanto más permisivos seamos, menos altura 
            // habrá que superar. Por tanto, la tolerancia
            // REDUCE la altura de la cabeza (Posicion Y).
            if ((Head.Position.Y * (1.0 - this.tolerancia)) >= 0)
            {
                alturaASuperar = Head.Position.Y * (1.0 - this.tolerancia);
            }
            else
            {
                alturaASuperar = Head.Position.Y * (1.0 + this.tolerancia);
            }

            double XHead;

            if (Wrist.JointType == JointType.WristLeft)
            {
                if ((Head.Position.X * (1.0 - this.tolerancia)) >= 0)
                {
                    XHead = Head.Position.X * (1.0 - this.tolerancia);
                }
                else
                {
                    XHead = Head.Position.X * (1.0 + this.tolerancia);
                }
            }
            else
            {
                if ((Head.Position.X * (1.0 - this.tolerancia)) >= 0)
                {
                    XHead = Head.Position.X * (1.0 + this.tolerancia);
                }
                else
                {
                    XHead = Head.Position.X * (1.0 - this.tolerancia);
                }
            }

            if (Wrist.Position.Y > alturaASuperar)
            {
                // Ahora, la mano debe haber sobrepasado la cabeza en el eje X:
                if (Wrist.JointType == JointType.WristLeft)
                {
                    if (Wrist.Position.X >= XHead)
                        enPosicion = true;

                }
                else
                {
                    if (Wrist.Position.X <= XHead)
                        enPosicion = true;
                }
            }
            return enPosicion;
        }
    
        //NO PROBADOS:
        /// <summary>
        /// Método que comprueba si la mano está recta, apuntando hacia el suelo.
        /// </summary>
        /// <param name="Shoulder">Hombro del brazo que queremos comprobar </param>
        /// <param name="Elbow">Codo del brazo que queremos comprobar </param>
        /// <param name="Wrist">Muñeca del brazo que queremos comprobar </param>
        private bool manoAbajo(Joint Shoulder, Joint Elbow, Joint Wrist)
        {
            bool enPosicion;

            // Tenemos que comprobar que la espalda esté recta, lo que se traduce en que cada 
            // articulación de la espalda esté aproximadamente en la misma X y en la misma Z:
            double topeSuperior;
            double topeInferior;

            if ((Elbow.Position.X * (1.0 + this.tolerancia)) >= 0)
            {
                topeSuperior = Elbow.Position.X * (1.0 + this.tolerancia);
                topeInferior = Elbow.Position.X * (1.0 - this.tolerancia);
            }
            else
            {
                topeSuperior = Elbow.Position.X * (1.0 - this.tolerancia);
                topeInferior = Elbow.Position.X * (1.0 + this.tolerancia);
            }

            if (Shoulder.Position.X > topeSuperior || Wrist.Position.X > topeSuperior)
                enPosicion = false;
            else if (Shoulder.Position.X < topeInferior || Wrist.Position.X < topeInferior)
                enPosicion = false;
            else
            {
                if ((Elbow.Position.Z * (1.0 + this.tolerancia)) >= 0)
                {
                    topeSuperior = Elbow.Position.Z * (1.0 + this.tolerancia);
                    topeInferior = Elbow.Position.Z * (1.0 - this.tolerancia);
                }
                else
                {
                    topeSuperior = Elbow.Position.Z * (1.0 - this.tolerancia);
                    topeInferior = Elbow.Position.Z * (1.0 + this.tolerancia);
                }

                if (Elbow.Position.Z > topeSuperior || Wrist.Position.Z > topeSuperior)
                    enPosicion = false;
                else if (Elbow.Position.Z < topeInferior || Wrist.Position.Z < topeInferior)
                    enPosicion = false;
                else
                    enPosicion = true;
            }

            return enPosicion;
        }

        /// <summary>
        /// Método que comprueba si el brazo está en arco, con la mano está apoyada en la cadera
        /// </summary>
        /// <param name="Elbow">Codo del brazo que queremos comprobar </param>
        /// <param name="Wrist">Muñeca del brazo que queremos comprobar </param>
        /// <param name="Hip">Cadera del esqueleto </param>
        private bool manoEnCadera(Joint Elbow, Joint Wrist, Joint Hip)
        {
            bool enPosicion = false;

            // Tenemos que comprobar que la posición de la muñeca esté junto a la cadera. Para
            // ello ha de tener las mismas coordenadas en Z y en Y. La X debe ser similar también.
            // Para calcular una X aproximada, yo he utilizado la distancia de la muñeca al codo, de
            // forma que dependiendo de lo cerca que esté la persona y sus proporciones, varíe el margen.
            double topeSuperior;
            double topeInferior;
            
            if ((Wrist.Position.X * (1.0 + this.tolerancia)) >= 0)
            {
                topeSuperior = Wrist.Position.X * (1.0 + this.tolerancia);
                topeInferior = Wrist.Position.X * (1.0 - this.tolerancia);
            }
            else
            {
                topeInferior = Wrist.Position.X * (1.0 + this.tolerancia);
                topeSuperior = Wrist.Position.X * (1.0 - this.tolerancia);
            }
            double x = (double) (Elbow.Position.X - Wrist.Position.X);
            double y = (double) (Elbow.Position.Y - Wrist.Position.Y);
            double z = (double) (Elbow.Position.Z - Wrist.Position.Z);

            x = x * x;
            y = y * y;
            z = z * z;

            double margenCadera = Math.Sqrt(x + y + z);

            this.feedBack += string.Format("X Muñeca: X:{0:0.00} / Y:{1:0.00} \n", topeSuperior, topeInferior);
            this.feedBack += string.Format("X Cadera: X:{0:0.00} \n", Hip.Position.X);
            this.feedBack += string.Format("Margen y +/-Margen: X:{0:0.00} / Y:{1:0.00} / Z:{2:0.00} \n", margenCadera, (topeSuperior + margenCadera), (topeInferior - margenCadera));

            if (Hip.Position.X < (topeSuperior + margenCadera) && Hip.Position.X > (topeInferior - margenCadera))
            {
                if ((Wrist.Position.Y * (1.0 + this.tolerancia)) >= 0)
                {
                    topeSuperior = Wrist.Position.Y * (1.0 + this.tolerancia);
                    topeInferior = Wrist.Position.Y * (1.0 - this.tolerancia);
                }
                else
                {
                    topeInferior = Wrist.Position.Y * (1.0 + this.tolerancia);
                    topeSuperior = Wrist.Position.Y * (1.0 - this.tolerancia);
                }

                this.feedBack += string.Format("Y Muñeca: X:{0:0.00} / Y:{1:0.00} \n", topeSuperior, topeInferior);
                this.feedBack += string.Format("Y Cadera: X:{0:0.00} \n", Hip.Position.Y);

                if (Hip.Position.Y <= topeSuperior && Hip.Position.Y >= topeInferior)
                {
                    if ((Wrist.Position.Z * (1.0 + this.tolerancia)) >= 0)
                    {
                        topeSuperior = Wrist.Position.Z * (1.0 + this.tolerancia);
                        topeInferior = Wrist.Position.Z * (1.0 - this.tolerancia);
                    }
                    else
                    {
                        topeInferior = Wrist.Position.Z * (1.0 + this.tolerancia);
                        topeSuperior = Wrist.Position.Z * (1.0 - this.tolerancia);
                    }

                    if (Hip.Position.Z <= topeSuperior && Hip.Position.Z >= topeInferior)
                        enPosicion = true;
                }
                    
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
            /****************************************************************************/
            if (this.manoSobreCabeza(esqueleto.Joints[JointType.WristLeft], esqueleto.Joints[JointType.Head]))
                terminadoCorrectamente = true;

            /****************************************************************************/
            if (this.manoSobreCabeza(esqueleto.Joints[JointType.WristRight], esqueleto.Joints[JointType.Head]))
                terminadoCorrectamente = true;

             
             /****************************************************************************/
            if (this.manoAbajo(esqueleto.Joints[JointType.ShoulderLeft],esqueleto.Joints[JointType.ElbowLeft],esqueleto.Joints[JointType.WristLeft]))
                terminadoCorrectamente = true; 

            /****************************************************************************/
            if (this.manoEnCadera(esqueleto.Joints[JointType.ElbowLeft],esqueleto.Joints[JointType.WristLeft], esqueleto.Joints[JointType.HipLeft]))
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