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
        /// Esta variable booleana activa y desactiva los cálculos de saturación:
        /// </summary>
        private bool activarSaturacion;


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

                // Asignamos nuestro bitmap a la imagen que mostraremos
                this.ImagenColorStream.Source = this.colorBitmap;

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
            Skeleton[] esqueletos = null;

            using (SkeletonFrame frameEsqueletos = e.OpenSkeletonFrame())
            {
                if (frameEsqueletos != null)
                {
                    esqueletos = new Skeleton[frameEsqueletos.SkeletonArrayLength];
                    frameEsqueletos.CopySkeletonDataTo(esqueletos);
                }
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
            string path = Path.Combine(myPhotos, "KinectSnapshot-" + time + ".png");

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