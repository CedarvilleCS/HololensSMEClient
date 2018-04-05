using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Threading;
using WpfApplication1;

namespace ARTAPclient
{
    /// <summary>
    /// Asynchronously handle network connection to
    /// the server. In this case, the HoloLens.
    /// </summary>
    public class AsynchronousSocketListener
    {
        #region Fields

        /// <summary>
        /// IPEndPoint describing the host we are connecting to
        /// </summary>
        private readonly IPEndPoint _remoteEndPoint;

        /// <summary>
        /// The socket for the connection to take place on
        /// </summary>
        private Socket _client = new Socket(AddressFamily.InterNetwork,
                SocketType.Stream, ProtocolType.Tcp);

        /// <summary>
        /// Types of messages that can be sent and their
        /// coresponding message  integer codes
        /// </summary>
        private enum MessageType
        {
            Bitmap = 1,
            PositionIDRequest = 2,
            ArrowPlacement = 3,
            EraseMarkers = 4,
            Pdf = 5,
            EraseMarker = 6,
            PanoRequest = 10,
            LocationRequest = 11
            // request one of these every second
            // he'll send some information for the rectangle
            // look at imageposition
        }

        /// <summary>
        /// Handles timing for checking if the connection is alive
        /// </summary>
        private System.Timers.Timer _connectionAliveTimer;

        private byte[] _lengthBytes;
        private byte[] _headPositionBytes;
        private ImagePosition _headPosition;

        private PanoramaStateObject _panoramaState;
        private PanoramaWindow _panoramaWindow;
        public ImageSource panoImage = null;

        #endregion

        #region Constructor

        /// <summary>
        /// Constructor for Asynch Socket Listener
        /// </summary>
        /// <param name="remoteEndPoint">Remote end point to connect to</param>
        public AsynchronousSocketListener(IPEndPoint remoteEndPoint)
        {
            _remoteEndPoint = remoteEndPoint;
            _connectionAliveTimer = new System.Timers.Timer(5000);
            _connectionAliveTimer.Elapsed += ConnectionAliveTimerElapsed;
            _lengthBytes = new byte[4];
            _panoramaState = new PanoramaStateObject()
            {
                Panorama = new Panorama()
            };
        }

        #endregion

        #region Public Methods

        public void RequestHeadPosition()
        {
            Send(MessageType.LocationRequest, new byte[0]);
        }

        public void GetHeadPosition(byte[] headPositionData)
        {
            try
            {
                _client.BeginReceive(_headPositionBytes, 0, 44, 0, new AsyncCallback(AssignHeadPositionData), headPositionData);
            }
            catch (Exception)
            {
                MessageBox.Show("An error occurred receiving the Head position data from the HoloLens.",
                    "Network Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        public void AssignHeadPositionData(IAsyncResult ar)
        {
            _headPositionBytes = (byte[])ar.AsyncState;
            _client.EndReceive(ar);
            _headPosition = ImagePosition.FromByteArray(_headPositionBytes);
            if (_panoramaState.Panorama.ContainsPoint(_headPosition))
            {
                float[] pos = _panoramaState.Panorama.GetPositionOnPano(_headPosition);
                int a = 1;
                // do something with pos to view tracker
            }
            else
            {
                // reset view tracker
            }
        }

        /// <summary>
        /// Connect to the server (HoloLens)
        /// </summary>
        public void Connect()
        {
            // Connect to a remote device.
            try
            {
                // Connect to the remote endpoint.
                _client.BeginConnect(_remoteEndPoint,
                    new AsyncCallback(ConnectCallback), _client);

            }
            catch (Exception e)
            {
                Console.WriteLine(e.ToString());
            }
        }

        /// <summary>
        /// Close socket connection gracefully
        /// </summary>
        public void CloseConnection()
        {
            _connectionAliveTimer.Stop();
            _client.Close();
        }

        public void SendIpAddress(PanoramaWindow panoramaWindow)
        {
            _panoramaWindow = panoramaWindow;

            byte[] ipAddress = null;
            var host = Dns.GetHostEntry(Dns.GetHostName());
            foreach (var ip in host.AddressList)
            {
                if (ip.AddressFamily == AddressFamily.InterNetwork)
                {
                    ipAddress = Encoding.UTF8.GetBytes(ip.ToString());
                }
            }

            Send(MessageType.PanoRequest, ipAddress);
            ReceivePanorama(_panoramaState.Panorama);
        }

        public void ReceivePanorama(Panorama panorama)
        {
            try
            {
                _client.BeginReceive(_lengthBytes, 0, 4, 0, new AsyncCallback(ReceiveMessageLength), _panoramaState);
            }
            catch (Exception)
            {
                MessageBox.Show("An error occurred receiving the Panorama from the HoloLens.",
                    "Network Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        public void ReceiveMessageLength(IAsyncResult ar)
        {
            IsPanoDone = false;
            if (BitConverter.IsLittleEndian)
            {
                Array.Reverse(_lengthBytes);
            }

            var length = BitConverter.ToInt32(_lengthBytes, 0);
            _panoramaState.buffer = new byte[length];

            var bytesReceived = 0;
            var bytesRemaining = length;
            while (bytesReceived < bytesRemaining)
            {
                var numBytes = _client.Receive(_panoramaState.buffer, bytesReceived, bytesRemaining, SocketFlags.None);
                if (numBytes == 0)
                {
                    _panoramaState.buffer = null;
                    break;
                }

                bytesReceived += numBytes;
                bytesRemaining -= numBytes;
            }

            var panoImages = ParsePanoData(_panoramaState.buffer);
            var holoPano = new Panorama(panoImages[1]);
            _panoramaState.Panorama = new Panorama(panoImages[0]);
            using (var fileStream = new FileStream("HoloPano.png", FileMode.Create))
            {
                BitmapEncoder encoder = new PngBitmapEncoder();
                encoder.Frames.Add(BitmapFrame.Create(holoPano.Image));
                encoder.Save(fileStream);
            }

            IsPanoDone = true;
            _panoramaState.Panorama = new Panorama();

            ReceivePanorama(_panoramaState.Panorama);
        }

        /// <summary>
        /// Send bitmap to the server (HoloLens)
        /// </summary>
        /// <param name="bm">Bitmap object to send</param>
        public void SendBitmap(ImageSource image)
        {
            var bmpSrc = image as BitmapSource;
            var encoder = new JpegBitmapEncoder();
            var outputFrame = BitmapFrame.Create(bmpSrc);
            encoder.Frames.Add(outputFrame);
            encoder.QualityLevel = 100;

            byte[] imgData;
            using (var imgStream = new MemoryStream())
            {
                encoder.Save(imgStream);
                imgStream.Position = 0;
                imgData = imgStream.ToArray();
            }

            Send(MessageType.Bitmap, imgData);
        }

        public void SendPDF(PDFDocument document)
        {
            var imgData = document.ToByteArray();
            Send(MessageType.Pdf, imgData);
        }

        /// <summary>
        /// Sends information to place an arrow for the HoloLens viewer
        /// </summary>
        /// <param name="image">LocatableImage with placement data</param>
        public void SendArrowLocation(LocatableImage image)
        {
            if (image.Markers.Length > 0)
            {
                var marker = image.Markers.Last();
                if (!marker.Sent)
                {
                    byte[] width = GetShortBytesFromDouble(image.OriginalImage.Width);
                    byte[] height = GetShortBytesFromDouble(image.OriginalImage.Height);
                    byte[] direction = GetShortBytesFromInt((int)marker.Direction);

                    byte[] x = GetShortBytesFromDouble(marker.AbsoluteLocation.X);
                    byte[] y = GetShortBytesFromDouble(marker.AbsoluteLocation.Y);

                    byte[] color = { marker.Color.R, marker.Color.G, marker.Color.B };

                    byte[] message = CombineArrs(image.PositionID, width, height, x, y, direction, color);
                    Send(MessageType.ArrowPlacement, message);

                    marker.Sent = true;
                }
            }
        }

        public void EraseOneMarker(LocatableImage image)
        {
            Send(MessageType.EraseMarker, image.PositionID);
        }

        /// <summary>
        /// Erases all markers for a given image
        /// </summary>
        /// <param name="image">Image to erase relative to</param>
        public void SendEraseMarkers(LocatableImage image)
        {
            Send(MessageType.EraseMarkers, image.PositionID);
        }

        /// <summary>
        /// Erases all markers
        /// </summary>
        public void SendEraseMarkers()
        {
            Send(MessageType.EraseMarkers, new byte[0]);
        }

        /// <summary>
        /// Converts a double into short byte form
        /// </summary>
        /// <param name="d">Double to convert</param>
        /// <returns>Byte array of 16bit representation</returns>
        private byte[] GetShortBytesFromDouble(double d)
        {
            var s = (short)d;
            byte[] bytes = BitConverter.GetBytes(s);

            if (BitConverter.IsLittleEndian)
            {
                Array.Reverse(bytes);
            }

            return bytes;
        }

        private byte[] GetShortBytesFromInt(int i)
        {
            return GetShortBytesFromDouble(i);
        }

        /// <summary>
        /// Gets a location ID from the HoloLens for a locatable image
        /// </summary>
        /// <param name="image">Image to get ID for</param>
        public void RequestLocationID(LocatableImage image)
        {
            Send(MessageType.PositionIDRequest, new Byte[0]);
            ReceivePositionID(image);
        }

        public static byte[] Compress(byte[] data)
        {
            using (var ms = new MemoryStream())
            {
                using (var gzip = new GZipStream(ms, CompressionMode.Compress))
                {
                    gzip.Write(data, 0, data.Length);
                }
                data = ms.ToArray();
            }
            return data;
        }
        public static byte[] Decompress(byte[] data)
        {
            // the trick is to read the last 4 bytes to get the length
            // gzip appends this to the array when compressing
            var lengthBuffer = new byte[4];
            Array.Copy(data, data.Length - 4, lengthBuffer, 0, 4);
            int uncompressedSize = BitConverter.ToInt32(lengthBuffer, 0);
            var buffer = new byte[uncompressedSize];
            using (var ms = new MemoryStream(data))
            {
                using (var gzip = new GZipStream(ms, CompressionMode.Decompress))
                {
                    gzip.Read(buffer, 0, uncompressedSize);
                }
            }
            return buffer;
        }

        #endregion

        #region Private Methods

        /// <summary>
        /// Combines byte arrays passed in, code borrowed from:
        /// http://stackoverflow.com/questions/415291/best-way-to-combine-two-or-more-byte-arrays-in-c-sharp
        /// </summary>
        /// <param name="arrays">Arrays passed in to combine</param>
        /// <returns>A single combined byte array</returns>
        private byte[] CombineArrs(params byte[][] arrays)
        {
            byte[] rv = new byte[arrays.Sum(a => a.Length)];
            int offset = 0;
            foreach (byte[] array in arrays)
            {
                Buffer.BlockCopy(array, 0, rv, offset, array.Length);
                offset += array.Length;
            }

            return rv;
        }

        /// <summary>
        /// Sends message to the server (HoloLens)
        /// </summary>
        /// <param name="messageType">Type of message being sent</param>
        /// <param name="data">Message data to be sent</param>
        private void Send(MessageType messageType, byte[] data)
        {
            if (Connected)
            {
                ///
                /// Overall message length, +2 for length of message type
                /// 
                int totalLen = data.Length + 2;

                byte[] typeBytes = BitConverter.GetBytes((short)messageType);
                byte[] lengthBytes = BitConverter.GetBytes(totalLen);
                if (BitConverter.IsLittleEndian)
                {
                    Array.Reverse(typeBytes);
                    Array.Reverse(lengthBytes);
                }

                byte[] combinedData = CombineArrs(lengthBytes, typeBytes, data);
                try
                {
                    _client.BeginSend(combinedData, 0, combinedData.Length, SocketFlags.None,
                        new AsyncCallback(SendCallback), _client);
                }
                catch
                {
                    NotifyConnectionLost();
                }
            }
        }

        /// <summary>
        /// Begins async receive for a position ID from the HoloLens 
        /// </summary>
        /// <param name="image">Image to set the ID to when the receive is finished</param>
        private void ReceivePositionID(LocatableImage image)
        {
            try
            {
                var state = new StateObject();
                state.locatableImage = image;
                _client.BeginReceive(state.buffer, 0, StateObject.BUFFSIZE, 0,
                    new AsyncCallback(ReceiveCallback), state);
            }
            catch (Exception)
            {
                MessageBox.Show("An error occurred receiving the position ID from the HoloLens.",
                    "Network Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        /// <summary>
        /// Change connection status, stop checking if alive, and
        /// fire connection closed event
        /// </summary>
        private void NotifyConnectionLost()
        {
            Connected = false;
            _connectionAliveTimer.Stop();
            ConnectionClosed?.Invoke(this, new EventArgs());
        }

        #endregion

        #region Properties

        /// <summary>
        /// Is the socket connected?
        /// </summary>
        public bool Connected { get; private set; }
        public bool IsPanoDone { get; set; }


        #endregion

        #region Events

        /// <summary>
        /// Connection to server established
        /// </summary>
        public event EventHandler ConnectionEstablished;

        /// <summary>
        /// Connection to server closed or lost
        /// </summary>
        public event EventHandler ConnectionClosed;

        /// <summary>
        /// Connection timed out during connection attempt
        /// </summary>
        public event EventHandler ConnectionTimedOut;

        #endregion

        #region Event Handlers & Callbacks

        /// <summary>
        /// Fires every time the connection alive poll timer fires
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void ConnectionAliveTimerElapsed(object sender,
            System.Timers.ElapsedEventArgs e)
        {
            ///
            /// Returns true if the closed, reset, etc.
            ///
            bool part1 = _client.Poll(1000, SelectMode.SelectRead);
            ///
            ///Checks if there is anything available to read
            ///
            bool part2 = (_client.Available == 0);
            ///
            /// If it's closed/reset and there is nothing to read,
            /// the socket is not connected and is closed
            /// 
            if (part1 && part2)
            {
                NotifyConnectionLost();
            }
            else
            {
                Connected = true;
            }
        }

        /// <summary>
        /// Callback method for the connection taking place
        /// </summary>
        /// <param name="ar">IAsyncResult parameter</param>
        private void ConnectCallback(IAsyncResult ar)
        {
            try
            {
                _client.EndConnect(ar);
                Connected = true;
                ConnectionEstablished?.Invoke(this, new EventArgs());
            }
            catch (System.Net.Sockets.SocketException)
            {
                ConnectionTimedOut?.Invoke(this, new EventArgs());
            }
            ///
            /// Start polling to know the connection is alive
            ///
            _connectionAliveTimer.Start();
        }

        /// <summary>
        /// Callback for async send completed
        /// </summary>
        /// <param name="ar">IAsyncResult parameter</param>
        private void SendCallback(IAsyncResult ar)
        {
            int bytesSent = _client.EndSend(ar);
            ///
            /// For testing purposes
            ///
            Debug.WriteLine("Sent {0} bytes to server.", bytesSent);
        }

        /// <summary>
        /// Callback for async receive completed
        /// </summary>
        /// <param name="ar">IAsyncResult with state object</param>
        public void ReceiveCallback(IAsyncResult ar)
        {
            StateObject state = (StateObject)ar.AsyncState;

            //
            // TODO: Handle socket exception on HoloLens disconnect
            // Using rev 4606b2 on the HoloLens
            //
            _client.EndReceive(ar);

            state.locatableImage.PositionID = new byte[4];
            Array.Copy(state.buffer, 6, state.locatableImage.PositionID, 0, 4);
            if (BitConverter.IsLittleEndian)
            {
                Array.Reverse(state.locatableImage.PositionID);
            }
        }

        private List<PanoImage>[] ParsePanoData(byte[] data)
        {
            var messageType = SubArray(data, 0, 2);
            var panoImages = new List<PanoImage>();
            var screenshotImages = new List<PanoImage>();
            var decompressedData = SubArray(data, 2, data.Length - 2);
            var dataPosition = 0;
            for (var i = 0; i < 10; i++)
            {
                var lengthBytes = SubArray(decompressedData, dataPosition, 4);
                dataPosition += 4;
                if (BitConverter.IsLittleEndian)
                {
                    Array.Reverse(lengthBytes);
                }
                var panoLength = BitConverter.ToInt32(lengthBytes, 0);
                var imageBytes = SubArray(decompressedData, dataPosition, panoLength);
                PanoImage pImg = PanoImage.FromByteArray(imageBytes);
                if (i < 5)
                {
                    panoImages.Add(pImg);
                }
                else
                {
                    screenshotImages.Add(pImg);
                }


                dataPosition += panoLength;
            }

            return new List<PanoImage>[] { panoImages, screenshotImages };
        }

        public static byte[] SubArray(byte[] data, int index, int length)
        {
            var result = new byte[length];
            Array.Copy(data, index, result, 0, length);
            return result;
        }

        #endregion
    }

    /// <summary>
    /// Used for asynch network receive to pass data
    /// </summary>
    public class StateObject
    {
        /// <summary>
        /// Size of data buffer
        /// </summary>
        public const int BUFFSIZE = 10;

        /// <summary>
        /// Buffer data is stored in from read
        /// </summary>
        public byte[] buffer = new byte[BUFFSIZE];

        /// <summary>
        /// Image the location ID corresponds with
        /// </summary>
        public LocatableImage locatableImage;
    }

    public class PanoramaStateObject
    {
        public int expectedDataLength { get; set; }
        public byte[] buffer { get; set; }
        public Panorama Panorama { get; set; }
    }
}
