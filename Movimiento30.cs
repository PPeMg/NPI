using System;
using Microsoft.Kinect;

namespace Microsoft.Samples.Kinect.SkeletonBasics
{
    class Movimiento30
    {
        //Indica si el movimiento se ha iniciado o no.
        private bool iniciado;

        //Distancia de movimiento en el eje -X
        private float distancia;

        //Tolerancia admitida
        private float tolerancia;

        //Constructor con dos parámetros, que indican la distancia de movimiento y la tolerancia admitida:
        public Movimiento30(float dist, float tol)
        {
            //Al principio, el movimiento está no iniciado:
            this.iniciado = false;

            //Distancia a recorrer:
            this.distancia = dist;

            //La tolerancia es el segundo parámetro que acepta el constructor:
            this.tolerancia = tol;
        }

        //Comprobar que la pierna izquierda está recta y en reposo:
        private bool piernaEnReposo(Skeleton skel, ref float ankleZ_inicial)
        {
            bool posCorrecta = false;
            Joint[] articulacionesPierna = new Joint[3];
            articulacionesPierna[0] = skel.Joints[JointType.HipLeft];
            articulacionesPierna[1] = skel.Joints[JointType.KneeLeft];
            articulacionesPierna[2] = skel.Joints[JointType.AnkleLeft];

            //Almacena true si las tres articulaciones de la pierna izquierda tengan más o menos la misma profundidad
            bool alineadas_horizontal = (articulacionesPierna[0].Position.Z >= articulacionesPierna[1].Position.Z * (1 - tolerancia)) && (articulacionesPierna[0].Position.Z <= articulacionesPierna[1].Position.Z * (1 + tolerancia)) && (articulacionesPierna[1].Position.Z >= articulacionesPierna[2].Position.Z * (1 - tolerancia)) && (articulacionesPierna[1].Position.Z <= articulacionesPierna[2].Position.Z * (1 + tolerancia));
            //Almacena true si las tres articulaciones de la pierna izquierda estén alineadas en horizontal:
            bool alineadas_profundidad = (articulacionesPierna[0].Position.X >= articulacionesPierna[1].Position.X * (1 - tolerancia)) && (articulacionesPierna[0].Position.X <= articulacionesPierna[1].Position.X * (1 + tolerancia)) && (articulacionesPierna[1].Position.X >= articulacionesPierna[2].Position.X * (1 - tolerancia)) && (articulacionesPierna[1].Position.X <= articulacionesPierna[2].Position.X * (1 + tolerancia));

            if (alineadas_horizontal && alineadas_profundidad)
                posCorrecta = true;

            ankleZ_inicial = articulacionesPierna[2].Position.Z;

            return posCorrecta;
        }

        public bool movimientoCorrecto(Skeleton skel)
        {
            bool movimiento_correcto = false;
            float posicion_inicial_tobillo = 0.0f;
            float distancia_recorrida, z_actual_tobillo;

            Joint[] articulacionesPierna = new Joint[3];
            articulacionesPierna[0] = skel.Joints[JointType.HipLeft];
            articulacionesPierna[1] = skel.Joints[JointType.KneeLeft];
            articulacionesPierna[2] = skel.Joints[JointType.AnkleLeft];

            if(this.iniciado == false){ 
                if(this.piernaEnReposo(skel, ref posicion_inicial_tobillo))
                    this.iniciado = true;
            } 
            
            else {
                z_actual_tobillo = articulacionesPierna[2].Position.Z;
                
                //Este If comprueba que el movimiento sea en el sentido negativo del eje:
                if (z_actual_tobillo <= posicion_inicial_tobillo)
                {

                    distancia_recorrida = Math.Abs(z_actual_tobillo - posicion_inicial_tobillo);

                    if ((distancia_recorrida > this.distancia * (1 - this.tolerancia)) && (distancia_recorrida < this.distancia * (1 + this.tolerancia)))
                        movimiento_correcto = true;
                }
            }
            return movimiento_correcto;
        }
    }
}
