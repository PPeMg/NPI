namespace Microsoft.Samples.Kinect.ColorBasics
{
    using System;
    using System.Globalization;
    using System.Windows;
    using System.Windows.Media;
    using System.Windows.Media.Imaging;
    
    // Mis agregados:
    using Microsoft.Kinect;
    using System.IO;
    using System.Windows.Shapes;

    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        /// <summary>
        /// Kinect que vamos a utilizar.
        /// </summary>
        private KinectSensor kinectConectado;

        /// <summary>
        /// Bitmap que contendrá la información de color recibida del kinectActual.
        /// </summary>
        private WriteableBitmap colorBitmap;

        /// <summary>
        /// Buffer que usaremos para almacenar temporalmente los colores recibidos del kinectActual.
        /// </summary>
        private byte[] colorPixels;

        /// <summary>
        /// Esta variable almacenará el valor del slider que controla la saturación:
        /// </summary>
        private int valorSaturacion;

        /// <summary>
        /// Esta variable almacenará un booleano que nos servirá para hacer más eficiente el proceso de
        /// activar y desactivar el modo esqueleto:
        /// </summary>
        private bool cambioModoEsqueleto = false;

        /// <summary>
        /// Initializes a new instance of the MainWindow class.
        /// </summary>
        public MainWindow()
        {
            InitializeComponent();
        }

        /// <summary>
        /// Execute startup tasks
        /// </summary>
        /// <param name="sender">object sending the event</param>
        /// <param name="e">event arguments</param>
        private void WindowLoaded(object sender, RoutedEventArgs e)
        {
            //Recorremos el vector de Sensores Kinect y elegimos el primero que esté conectado:
            foreach (var kinectActual in KinectSensor.KinectSensors)
            {
                if (kinectActual.Status == KinectStatus.Connected)
                {
                    this.kinectConectado = kinectActual;
                    break;
                }
            }

            //Si hay un sensor kinect conectado:
            if (null != this.kinectConectado)
            {
                // Habilitamos el Stream de Color
                this.kinectConectado.ColorStream.Enable(ColorImageFormat.RgbResolution640x480Fps30);

                // Reservamos memoria para el buffer.
                // El valor PixelDataLength nos indica el tamaño del buffer. Utilizamos el vector de byte porque estamos usando
                // el stream en RGBA en el que cada dato ocupa 4 bytes (uno para cada color y el último para el canal alfa).
                this.colorPixels = new byte[this.kinectConectado.ColorStream.FramePixelDataLength];

                // Creamos el bitmap que usaremos para mostrar en pantalla:
                this.colorBitmap = new WriteableBitmap(
                    this.kinectConectado.ColorStream.FrameWidth,                // Anchura del frame actual
                    this.kinectConectado.ColorStream.FrameHeight,               // Altura del frame actual
                    96.0,                                                       // DPI X
                    96.0,                                                       // DPI Y
                    PixelFormats.Bgr32,                                         // Formato de los Píxeles
                    null                                                        // Paleta del Bitmap
                );

                // Creamos un pincel de imagen a partir de nuestro bitmap de color para pintar el fondo del canvas. De esta forma,
                // Cuando pintemos el esqueleto, lo haremos sobre la imagen de color, de forma que podremos ver ambas imágenes superpuestas.
                this.canvasSalidaKinect.Background = new ImageBrush(this.colorBitmap);

                // Añadimos un manejador para cuando el kinect esté recibiendo imágenes del Color Stream
                this.kinectConectado.ColorFrameReady += this.SensorColorFrameReady;

                // Activamos el Stream de Esqueletos
                this.kinectConectado.SkeletonStream.Enable();

                // Añadimos un manejador para controlar el Flujo de Esqueletos
                this.kinectConectado.SkeletonFrameReady += new EventHandler<SkeletonFrameReadyEventArgs>(kinectConectado_SkeletonFrameReady);

                // Iniciamos el sensor:
                try
                {
                    this.kinectConectado.Start();
                }
                catch (IOException)
                {
                    this.kinectConectado = null;
                }
            }
                 
            else
            {
                this.textoEstadoKinect.Text = Properties.Resources.NoKinectReady;
            }
        }

        /// <summary>
        /// Execute shutdown tasks
        /// </summary>
        /// <param name="sender">object sending the event</param>
        /// <param name="e">event arguments</param>
        private void WindowClosing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            //Si hay algún kinect activado, pararlo.
            if (null != this.kinectConectado)
            {
                this.kinectConectado.Stop();
            }
        }

        /// <summary>
        /// Manejador para cuando el kinect esté preparado para enviar el stream de color
        /// </summary>
        /// <param name="sender">object sending the event</param>
        /// <param name="e">event arguments</param>
        private void SensorColorFrameReady(object sender, ColorImageFrameReadyEventArgs e)
        {
            // El using nos permite crear un objeto que será eliminado por el compilador cuando hayamos terminado de utilizarlo
            // (es decir, cuando acabe el bloque de using). De esta forma, usaremos cada imagen y luego la eliminaremos, para que
            // deje espacio para las siguientes:
            using (ColorImageFrame colorStreamFrame = e.OpenColorImageFrame())
            {
                // Comprobamos que la imagen recibida no esté vacía:
                if (colorStreamFrame != null)
                {
                    // Copiamos los datos al vector que hemos declarado antes.
                    colorStreamFrame.CopyPixelDataTo(this.colorPixels);

                    // Si está activado el checkbox de control de Saturación:
                    if ((bool)SaturacionCheckbox.IsChecked)
                    {
                        int valorNuevo = 0;

                        for (int i = 0; i < colorStreamFrame.PixelDataLength; i += 4)
                        {
                            // Cambiamos el color azul:
                            valorNuevo = (int)this.colorPixels[i];
                            if (valorNuevo + this.valorSaturacion >= 255)
                                valorNuevo = 255;
                            else if (valorNuevo + this.valorSaturacion <= 0)
                                valorNuevo = 0;
                            else
                                valorNuevo += this.valorSaturacion;

                            this.colorPixels[i] = (byte)valorNuevo;

                            // Cambiamos el color verde:
                            valorNuevo = (int)this.colorPixels[i + 1];
                            if (valorNuevo + this.valorSaturacion >= 255)
                                valorNuevo = 255;
                            else if (valorNuevo + this.valorSaturacion <= 0)
                                valorNuevo = 0;
                            else
                                valorNuevo += this.valorSaturacion;

                            this.colorPixels[i + 1] = (byte)valorNuevo;

                            // Y ahora cambiamos el color rojo:
                            valorNuevo = (int)this.colorPixels[i + 2];
                            if (valorNuevo + this.valorSaturacion >= 255)
                                valorNuevo = 255;
                            else if (valorNuevo + this.valorSaturacion <= 0)
                                valorNuevo = 0;
                            else
                                valorNuevo += this.valorSaturacion;

                            this.colorPixels[i + 2] = (byte)valorNuevo;
                        }
                    }

                    // Escribimos la imagen en nuestro bitmap. De esta forma, se mostrará en pantalla.
                    this.colorBitmap.WritePixels(
                        new Int32Rect(0, 0, this.colorBitmap.PixelWidth, this.colorBitmap.PixelHeight),
                        this.colorPixels,
                        this.colorBitmap.PixelWidth * sizeof(int),
                        0);
                }
            }
        }

        /// <summary>
        /// Manejador del flujo de esqueletos
        /// </summary>
        /// <param name="sender">object sending the event</param>
        /// <param name="e">event arguments</param>
        void kinectConectado_SkeletonFrameReady(object sender, SkeletonFrameReadyEventArgs e)
        {
            //Comprobamos si está activado el checkbox de Esqueleto. Si no lo está, no mostramos el esqueleto:
            if ((bool)SkeletonCheckbox.IsChecked)
            {
                // Esta condición simplemente sirve para no ejecutar continuamente el vaciado del canvas cuando se
                // desactiva el checkbox:
                if (!this.cambioModoEsqueleto)
                    this.cambioModoEsqueleto = true;

                // Vaciamos el Canvas del esqueleto actual:
                this.canvasSalidaKinect.Children.Clear();
                // Vector que contendrá
                Skeleton[] esqueletos = null;

                using (SkeletonFrame frameEsqueletos = e.OpenSkeletonFrame())
                {
                    // Comprobamos que no hayamos recibido un frame vacío:
                    if (frameEsqueletos != null)
                    {
                        // Rellenamos el vector de esqueletos con los datos del frame:
                        esqueletos = new Skeleton[frameEsqueletos.SkeletonArrayLength];
                        frameEsqueletos.CopySkeletonDataTo(esqueletos);
                    }

                    // Si hay algún esqueleto recibido:
                    if (esqueletos != null)
                    {
                        // Para cada esqueleto
                        foreach (Skeleton esq in esqueletos)
                        {
                            // Si todo el esqueleto está siendo detectado (TRACKED)
                            if (esq.TrackingState == SkeletonTrackingState.Tracked)
                            {
                                pintarEsqueleto(esq, Colors.Violet, Colors.Black);
                            }
                        }
                    }
                    // Si no hay ningún esqueleto en los recibidos, no lo pintamos.
                    else return;
                }
            }
            else
            {
                if (this.cambioModoEsqueleto)
                {
                    this.canvasSalidaKinect.Children.Clear();
                    this.cambioModoEsqueleto = false;
                }
            }

        }

        /// <summary>
        /// Pinta una articulacion en el canvas. Calcula también el mapeo de su posición:
        /// </summary>
        /// <param name="articulacion">Articulacíon a pintar.</param>
        /// <param name="puntoMapeo">Variable donde guardaremos el punto de imagen de color tras ser mapeado.</param>
        /// <param name="colorArticulacion">Color en que se pintará la articulación.</param>
        void pintarArticulacion(Joint articulacion, Color colorArticulacion)
        {
            // Mapeamos la articulación para obtener su posición en la imagen:
            ColorImagePoint puntoMapeo = this.kinectConectado.CoordinateMapper.MapSkeletonPointToColorPoint(articulacion.Position, ColorImageFormat.RgbResolution640x480Fps30);

            // Creamos la Elipse que añadiremos al canvas:
            Ellipse art = new Ellipse();
            
            // Asignamos las propiedades de la elipse. Nos interesa que sea igual de alta que de ancha para que sea un círculo. 
            // También nos interesa que sea relativamente gruesa, para que se vea bien. El color será el que obtenemos por parámetro.
            art.Width = 14;
            art.Height = 14;
            art.Stroke = new SolidColorBrush(colorArticulacion);
            art.StrokeThickness = 10;

            // Añadimos unos márgenes a la Elipse, para que se situe en la posición correcta dentro del Canvas:
            art.Margin = new Thickness(puntoMapeo.X, puntoMapeo.Y, 0, 0);

            // Por úlitmo, añadimos la elipse al canvas para pintarla.
            this.canvasSalidaKinect.Children.Add(art);
        }

        /// <summary>
        /// Pinta un hueso en el canvas:
        /// </summary>
        /// <param name="articulacion1">Primera articulacíon implicada en el hueso.</param>
        /// <param name="articulacion2">Segunda articulacíon implicada en el hueso.</param>
        /// <param name="colorHueso">Color en que se pintará el hueso.</param>
        void pintarHueso(Joint articulacion1, Joint articulacion2, Color colorHueso)
        {
            // Declaramos dos puntos de color, que utlizaremos para guardar el resultado del mapeo:
            ColorImagePoint a1, a2;

            // Mapeamos ambas articulaciones. Utilizaremos mapeo de esqueleto a color, que es lo que necesitamos:
            a1 = this.kinectConectado.CoordinateMapper.MapSkeletonPointToColorPoint(articulacion1.Position, ColorImageFormat.RgbResolution640x480Fps30);
            a2 = this.kinectConectado.CoordinateMapper.MapSkeletonPointToColorPoint(articulacion2.Position, ColorImageFormat.RgbResolution640x480Fps30);

            // Ahora creamos una línea para añadirla al canvas. Esta línea sera nuestro hueso. Para ello,
            // añadimos las dos coordenadas de cada articulación como los dos extremos de la línea:
            Line hueso = new Line();
            hueso.X1 = a1.X;
            hueso.X2 = a2.X;
            hueso.Y1 = a1.Y;
            hueso.Y2 = a2.Y;

            // Definimos otros datos de la línea, como el grosor y el color:
            hueso.Stroke = new SolidColorBrush(colorHueso);
            hueso.StrokeThickness = 4;

            //Por último, añadimos la línea a nuestro Canvas:
            this.canvasSalidaKinect.Children.Add(hueso);
        }

        /// <summary>
        /// Pinta un esqueleto completo. Para evitar que se olvide ningún hueso, pinta de fuera hacia dentro 
        /// y de arriba a abajo todas las partes del cuerpo. Se asume que el esqueleto está correcto, hay que 
        /// realizar la comprobación antes de llamar a la función:
        /// </summary>
        /// <param name="esqueleto">Esqueleto que queremos pintar</param>
        void pintarEsqueleto(Skeleton esqueleto, Color colorHuesos, Color colorArticulaciones)
        {
            // Pintamos la parte central del esqueleto, la columna vertebral:
            pintarHueso(esqueleto.Joints[JointType.Head], esqueleto.Joints[JointType.ShoulderCenter], colorHuesos);
            pintarHueso(esqueleto.Joints[JointType.ShoulderCenter], esqueleto.Joints[JointType.Spine], colorHuesos);
            pintarHueso(esqueleto.Joints[JointType.Spine], esqueleto.Joints[JointType.HipCenter], colorHuesos);

            // Ahora pintamos el brazo izquierdo:
            pintarHueso(esqueleto.Joints[JointType.HandLeft], esqueleto.Joints[JointType.WristLeft], colorHuesos);
            pintarHueso(esqueleto.Joints[JointType.WristLeft], esqueleto.Joints[JointType.ElbowLeft], colorHuesos);
            pintarHueso(esqueleto.Joints[JointType.ElbowLeft], esqueleto.Joints[JointType.ShoulderLeft], colorHuesos);
            pintarHueso(esqueleto.Joints[JointType.ShoulderLeft], esqueleto.Joints[JointType.ShoulderCenter], colorHuesos);

            // Seguidamente, pintamos el brazo derecho:
            pintarHueso(esqueleto.Joints[JointType.HandRight], esqueleto.Joints[JointType.WristRight], colorHuesos);
            pintarHueso(esqueleto.Joints[JointType.WristRight], esqueleto.Joints[JointType.ElbowRight], colorHuesos);
            pintarHueso(esqueleto.Joints[JointType.ElbowRight], esqueleto.Joints[JointType.ShoulderRight], colorHuesos);
            pintarHueso(esqueleto.Joints[JointType.ShoulderRight], esqueleto.Joints[JointType.ShoulderCenter], colorHuesos);

            // A continuación, pintamos la pierna izquierda
            pintarHueso(esqueleto.Joints[JointType.FootLeft], esqueleto.Joints[JointType.AnkleLeft], colorHuesos);
            pintarHueso(esqueleto.Joints[JointType.AnkleLeft], esqueleto.Joints[JointType.KneeLeft], colorHuesos);
            pintarHueso(esqueleto.Joints[JointType.KneeLeft], esqueleto.Joints[JointType.HipLeft], colorHuesos);
            pintarHueso(esqueleto.Joints[JointType.HipLeft], esqueleto.Joints[JointType.HipCenter], colorHuesos);

            // Por último, pintamos la pierna derecho:
            pintarHueso(esqueleto.Joints[JointType.FootRight], esqueleto.Joints[JointType.AnkleRight], colorHuesos);
            pintarHueso(esqueleto.Joints[JointType.AnkleRight], esqueleto.Joints[JointType.KneeRight], colorHuesos);
            pintarHueso(esqueleto.Joints[JointType.KneeRight], esqueleto.Joints[JointType.HipRight], colorHuesos);
            pintarHueso(esqueleto.Joints[JointType.HipRight], esqueleto.Joints[JointType.HipCenter], colorHuesos);

            // Y para acabar, mostramos todas las articulaciones sobre los huesos:
            foreach (Joint art in esqueleto.Joints)
            {
                pintarArticulacion(art, colorArticulaciones);
            }
        }

        /// <summary>
        /// Manejador para cuando se hace click en el botón de captura de pantalla:
        /// </summary>
        /// <param name="sender">object sending the event</param>
        /// <param name="e">event arguments</param>
        private void ButtonScreenshotClick(object sender, RoutedEventArgs e)
        {
            if (null == this.kinectConectado)
            {
                this.textoEstadoKinect.Text = Properties.Resources.ConnectDeviceFirst;
                return;
            }

            // Crea un codificador para guardar el bitmap en el formato de imagen deseado. En nuestro caso, utilizaremos
            // un codificador png.
            BitmapEncoder encoder = new PngBitmapEncoder();

            // Añadimos el la imagen actual del flujo al codificador.
            encoder.Frames.Add(BitmapFrame.Create(this.colorBitmap));
            
            // Añadimos la fecha con el formato deseado a un string, lo que nos servirá para añadirla al nombre del archivo .png.
            // Esto puede resultarnos útil para ordenar capturas o para llevar un registro.
            string time = System.DateTime.Now.ToString("hh'-'mm'-'ss", CultureInfo.CurrentUICulture.DateTimeFormat);

            // Aquí guardamos la dirección de la carpeta "Mis Imágenes" del SO actual.
            string myPhotos = Environment.GetFolderPath(Environment.SpecialFolder.MyPictures);

            // En este string encadenamos los dos anteriores dándole el formato requerido.
            string path = System.IO.Path.Combine(myPhotos, "KinectSnapshot-" + time + ".png");

            // Por último guardamos la imagen en un archivo. Para ello creamos un flujo de datos indicando en el path la ruta 
            // dónde guardaremos nuestra imagen.
            try
            {
                using (FileStream fs = new FileStream(path, FileMode.Create))
                {
                    encoder.Save(fs);
                }

                this.textoEstadoKinect.Text = string.Format(CultureInfo.InvariantCulture, "{0} {1}", Properties.Resources.ScreenshotWriteSuccess, path);
            }
            catch (IOException)
            {
                this.textoEstadoKinect.Text = string.Format(CultureInfo.InvariantCulture, "{0} {1}", Properties.Resources.ScreenshotWriteFailed, path);
            }
        }

        /// <summary>
        /// Manejador para cuando cambie el valor de saturación:
        /// <param name="sender">object sending the event</param>
        /// <param name="e">event arguments</param>
        private void SaturacionSlider_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            // Almacenamos el valor de saturación del slider.
            this.valorSaturacion = (int)SaturacionSlider.Value;
        }

    }
}