using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Windows.Forms;
using System.IO;
using System.Data.OleDb;
using System.Drawing.Imaging;


namespace sobel_filtering
{
    public partial class Form1 : Form
    {
        string _dataBasePath = "";
        string file_name;
        Bitmap Bit;
     
        public Form1()
        {
            InitializeComponent();
        }


        private void button1_Click(object sender, EventArgs e)
            {
            if (openFileDialog1.ShowDialog() == DialogResult.OK)
            {
                pictureBox1.Image = Image.FromFile(file_name);
                contrast_function();
                button1.Enabled = false;
                button_skincolor.Enabled = true;
                
            }

        }

        private void contrast_function()
        {
            double nContrast = 30;
            double pixel = 0, contrast = (100.0 + nContrast) / 100.0;

            contrast *= contrast;

            int red, green, blue;
            Bitmap b = new Bitmap(pictureBox1.Image);

            // GDI+ still lies to us - the return format is BGR, NOT RGB.
            BitmapData bmData = b.LockBits(new Rectangle(0, 0, b.Width, b.Height), ImageLockMode.ReadWrite, PixelFormat.Format24bppRgb);
            //Stride=No Of Pixel Per Row
            int stride = bmData.Stride;
            ///<summary> stores the address of the first pixel of the pixel row scan</summary>
            System.IntPtr Scan0 = bmData.Scan0; 

            unsafe
            {
                //First converts the IntPtr to a void pointer. Then to a byte pointer. This is unsafe code
                ///<summary> stores the address of the pointer 'Scan0'</summary>
                byte* p = (byte*)(void*)Scan0;

                int nOffset = stride - b.Width * 3;

                for (int y = 0; y < b.Height; ++y)
                {
                    for (int x = 0; x < b.Width; ++x)
                    {
                        blue = p[0];
                        green = p[1];
                        red = p[2];

                        pixel = red / 255.0;
                        pixel -= 0.5;
                        pixel *= contrast;
                        pixel += 0.5;
                        pixel *= 255;
                        if (pixel < 0) pixel = 0;
                        if (pixel > 255) pixel = 255;
                        p[2] = (byte)pixel;

                        pixel = green / 255.0;
                        pixel -= 0.5;
                        pixel *= contrast;
                        pixel += 0.5;
                        pixel *= 255;
                        if (pixel < 0) pixel = 0;
                        if (pixel > 255) pixel = 255;
                        p[1] = (byte)pixel;

                        pixel = blue / 255.0;
                        pixel -= 0.5;
                        pixel *= contrast;
                        pixel += 0.5;
                        pixel *= 255;
                        if (pixel < 0) pixel = 0;
                        if (pixel > 255) pixel = 255;
                        p[0] = (byte)pixel;

                        p += 3;
                        //Logger.Log("blue:" + blue + ",green:" + green + ",red:" + red);
                    }
                    p += nOffset;
                }
            }

            b.UnlockBits(bmData);
            pictureBox1.Image = (Image)b;

        }

        private void openFileDialog1_FileOk(object sender, CancelEventArgs e)
        {
            file_name = openFileDialog1.FileName;
        }

       
        bool[][] visited;
        int[][] big;

        Queue<int> queue_i;// = new Queue<int>(capacity);
        Queue<int> queue_j;// = new Queue<int>(capacity);

        int first_i;
        int first_j;

        private int find_first_neighbour(int x, int y)
        {
            int w = Bit.Width;
            int h = Bit.Height;

            if (x - 1 >= 0)
                if (!visited[x - 1][y] && Bit.GetPixel(x - 1, y).B == 0 && Bit.GetPixel(x - 1, y).R == 0 && Bit.GetPixel(x - 1, y).G == 0 && big[x - 1][y] == 0)
                {//when east pixel of black color is not visited
                    first_i = x - 1;
                    first_j = y;
                    return 1;
                }
            if (y - 1 >= 0)
                if (!visited[x][y - 1] && Bit.GetPixel(x, y - 1).B == 0 && Bit.GetPixel(x, y - 1).R == 0 && Bit.GetPixel(x, y - 1).G == 0 && big[x][y - 1] == 0)
                {//when north pixel of black color is not visited
                    first_i = x;
                    first_j = y - 1;
                    return 1;
                }
            if (x + 1 < w)
                if (!visited[x + 1][y] && Bit.GetPixel(x + 1, y).B == 0 && Bit.GetPixel(x + 1, y).R == 0 && Bit.GetPixel(x + 1, y).G == 0 && big[x + 1][y] == 0)
                {//when west pixel of black color is not visited
                    first_i = x + 1;
                    first_j = y;
                    return 1;
                }
            if (y + 1 < h)
                if (!visited[x][y + 1] && Bit.GetPixel(x, y + 1).B == 0 && Bit.GetPixel(x, y + 1).R == 0 && Bit.GetPixel(x, y + 1).G == 0 && big[x][y + 1] == 0)
                {//when south pixel of black color is not visited
                    first_i = x;
                    first_j = y + 1;
                    return 1;
                }


            if (x - 1 >= 0 && y - 1 >= 0)
                if (!visited[x - 1][y - 1] && Bit.GetPixel(x - 1, y - 1).B == 0 && Bit.GetPixel(x - 1, y - 1).R == 0 && Bit.GetPixel(x - 1, y - 1).G == 0 && big[x - 1][y - 1] == 0)
                {//north east pixel
                    first_i = x - 1;
                    first_j = y - 1;
                    return 1;
                }
            if (x + 1 < w && y - 1 >= 0)
                if (!visited[x + 1][y - 1] && Bit.GetPixel(x + 1, y - 1).B == 0 && Bit.GetPixel(x + 1, y - 1).R == 0 && Bit.GetPixel(x + 1, y - 1).G == 0 && big[x + 1][y - 1] == 0)
                {//north west pixel
                    first_i = x + 1;
                    first_j = y - 1;
                    return 1;
                }
            if (x + 1 < w && y + 1 < h)
                if (!visited[x + 1][y + 1] && Bit.GetPixel(x + 1, y + 1).B == 0 && Bit.GetPixel(x + 1, y + 1).R == 0 && Bit.GetPixel(x + 1, y + 1).G == 0 && big[x + 1][y + 1] == 0)
                {//south west pixel
                    first_i = x + 1;
                    first_j = y + 1;
                    return 1;
                }
            if (x - 1 >= 0 && y + 1 < h)
                if (!visited[x - 1][y + 1] && Bit.GetPixel(x - 1, y + 1).B == 0 && Bit.GetPixel(x - 1, y + 1).R == 0 && Bit.GetPixel(x - 1, y + 1).G == 0 && big[x - 1][y + 1] == 0)
                {//south east pixel
                    first_i = x - 1;
                    first_j = y + 1;
                    return 1;
                }

            return -1;

        }
        int count_region;
        int[] countt;

        private void BFS(int i, int j)
        {

            visited[i][j] = true;
            queue_i.Enqueue(i);
            queue_j.Enqueue(j);

            int w;
            while (queue_i.Count != 0)
            {
                i = queue_i.Dequeue(); //deque
                j = queue_j.Dequeue(); // deque

                big[i][j] = count_region;//assaigning tag for connected region
                countt[count_region]++;
                w = find_first_neighbour(i, j); //find first neighbour...if no neighbour return -1

                while (w != -1) //visit all 8 neighbours
                {
                    if (!visited[first_i][first_j])//unvisited nodes
                    {
                        visited[first_i][first_j] = true;

                        queue_i.Enqueue(first_i);//enque
                        queue_j.Enqueue(first_j);//enque

                    }

                    w = find_first_neighbour(i, j);// again find first neighbour...if no neighbour return -1
                }

            }

        }
        private void button_connected_Click(object sender, EventArgs e)
        {
            connected_area();
            button_skincolor.Enabled = false;
            button_connected.Enabled = false;
            button2.Enabled = true;
           
           
        }

        private void connected_area()
        {


            Bit = (Bitmap)pictureBox2.Image.Clone();
            int capacity = Bit.Height * Bit.Width;
            queue_i = new Queue<int>(capacity);
            queue_j = new Queue<int>(capacity);

            countt = new int[capacity];
            visited = new bool[Bit.Width + 5][];

            #region initialization of visited boolean array
            for (int i = 0; i < Bit.Width + 5; i++)
            {
                visited[i] = new bool[Bit.Height + 5];
                for (int j = 0; j < Bit.Height + 5; j++)
                    visited[i][j] = false;
            }
            #endregion
            big = new int[Bit.Width + 5][];

            #region initialization of count region array
            for (int i = 0; i < Bit.Width + 5; i++)
            {
                big[i] = new int[Bit.Height + 5];
                for (int j = 0; j < Bit.Height + 5; j++)
                    big[i][j] = 0;
            }
            #endregion

            int max = 0, max_bit = 0;
            count_region = 1;
            for (int i = 0; i < Bit.Width; i++)
            {
                for (int j = 0; j < Bit.Height; j++)
                {
                    if (!visited[i][j] && (Bit.GetPixel(i, j).R == 0 && Bit.GetPixel(i, j).G == 0 && Bit.GetPixel(i, j).B == 0))
                    {
                        countt[count_region] = 0;
                        BFS(i, j);
                        if (max < countt[count_region])
                        {
                            max = countt[count_region];
                            max_bit = count_region;
                          
                        }
                        count_region++;

                    }
                }
            }


            Bitmap bmp = new Bitmap(pictureBox2.Image.Width, pictureBox2.Image.Height);

            int min_x = Bit.Width;
            int max_x = 0;
            int max_y = 0;
            int min_y = Bit.Height;

         
            for (int i = 0; i < Bit.Width; i++)
            {
                for (int j = 0; j < Bit.Height; j++)
                {
                    if (big[i][j] == max_bit)
                    {
                        bmp.SetPixel(i, j, Bit.GetPixel(i, j));

                        #region calculating max min x and y of shorted image frame
                        if (min_x >= i)
                            min_x = i;
                        if (max_x < i)
                            max_x = i;
                        if (min_y >= j)
                            min_y = j;
                        if (max_y < j)
                            max_y = j;
                        #endregion
                    }
                    else
                        bmp.SetPixel(i, j, Color.White);

                }
            }

            int w, h, t;
            double a, p;
            int flagforidentification = 0;

            if (max_x - min_x >= 30 && max_y - min_x >= 30)
            {
                min_x = min_x - 30;
                min_y = min_y - 30;
                max_x = max_x + 30;
                max_y = max_y + 30;
                flagforidentification = 1;
            }

            if (min_x < 0)
                min_x = 0;

            if (min_y < 0)
                min_y = 0;

            if (max_x > Bit.Width)
                max_x = Bit.Width;

            if (max_y > Bit.Height)
                max_y = Bit.Height;
            a = max_x - min_x;
            p = a * 0.12;
            t = Convert.ToInt16(p);
            if (flagforidentification == 1)
            {
                max_x -= t;
                min_x += t;
                min_y += t;
            }
            a = max_x - min_x;
            w = Convert.ToInt16(a);
            h = Convert.ToInt16(w * 1.5);
           
            if (h + min_y > max_y)
                h = max_y - min_y;


            Bitmap bbbb = new Bitmap(w, h);
            Bitmap pic1 = (Bitmap)pictureBox1.Image.Clone();
            pictureBox2.Image = Image.FromFile(file_name);
            Bitmap fre = (Bitmap)pictureBox2.Image;
            for (int i = min_x; i < max_x; i++)
            {
                for (int j = min_y; j < min_y + h; j++)
                {
                  
                    bbbb.SetPixel(i - min_x, j - min_y, fre.GetPixel(i, j));
                   
                    pic1.SetPixel(i, j, Color.Black);
                   
                }
            }

         

            pictureBox2.Image = (Bitmap)bbbb;
            pictureBox2.Invalidate();
            pictureBox1.Image = (Image)pic1;
            queue_i.Clear();
            queue_j.Clear();
        }

     
        private void skin_color_segmentation()
        {
            Bitmap bm = (Bitmap)pictureBox1.Image;
            Bitmap bmp = new Bitmap(pictureBox1.Image.Width, pictureBox1.Image.Height);           
            int min_x = bm.Width + 5;
            int max_x = 0;
            int max_y = 0;
            int min_y = bm.Height + 5;

            Color color = new Color();
            bool  R3, R4, s;
            R3 = R4 = s = false;
          

            for (int i = 0; i < bm.Width; i++)
            {
                for (int j = 0; j < bm.Height; j++)
                {
                    color = bm.GetPixel(i, j);
                                                                                       
                    if (color.R > color.G && color.G > color.B)
                        R3 = true;
                    else
                        R3 = false;

                    if ((color.R - color.G) >= 45)
                        R4 = true;
                    else
                        R4 = false;

                    if (R3 && R4)
                        s = true;
                    else
                        s = false;

                    if (s)                                    
                    {

                        bmp.SetPixel(i, j, Color.Black);
                        R3 = R4 = s = false;
                        //bmp.SetPixel(i, j, color);
                        #region finding face rectangle
                        /*
                         * finding the minimum co-ordinate and maximum co-ordinate xy
                         * of the image between the Cb and Cr threshold value region
                         */

                        if (i < bm.Width / 2 && i < min_x)
                        {
                            min_x = i;
                        }
                        if ((i >= bm.Width / 2 && i < bm.Width) && i > max_x)
                        {
                            max_x = i;
                        }

                        if (j < bm.Height / 2 && j < min_y)
                        {
                            min_y = j;
                        }
                        if ((j >= bm.Height / 2 && j < bm.Height) && j > max_y)
                        {
                            max_y = j;
                        }
                        #endregion
                    }
                    else
                        bmp.SetPixel(i, j, Color.White);
                }
            }

            pictureBox2.Image = (Bitmap)bmp;
            pictureBox2.Invalidate();
            
        }

     
    

        private void Form1_Load(object sender, EventArgs e)
        {
            _dataBasePath = Directory.GetCurrentDirectory() + @"\db1.mdb";
            richTextBox1.Text = "1st Step :\n" + "Select an image.";
            richTextBox2.Text = "2nd Step :\n" + "Skin Color Conversion.";
            richTextBox3.Text = "3rd Step :\n" + "Find the largest connected region.";
            richTextBox4.Text = "4th Step :\n" + "Check the probability to become a face.";
            button1.Enabled = true;
            button_skincolor.Enabled = false;
            button_connected.Enabled = false;
            button2.Enabled = false;
        }

        private void button_skincolor_Click_1(object sender, EventArgs e)
        {
            skin_color_segmentation();
             button_skincolor.Enabled = false;
            button_connected.Enabled = true;           
            
        }

        private void button2_Click_1(object sender, EventArgs e)
        {
            
            button2.Enabled = false;
            Bitmap b = new Bitmap(pictureBox2.Image);

            long newHeight = b.Height;
            long newWeight = b.Width;
            double Ratio = (Convert.ToDouble(newHeight) / Convert.ToDouble(newWeight));

            if ((Ratio >= 1.0 && Ratio <= 2.0) && newHeight >= 50 && newWeight >= 50)
            {
                Form2 a = new Form2();
                a.pic(b);
                a.DataBasePath(_dataBasePath);
                a.Show();
            }
            else
            {
                MessageBox.Show("This is not a Human Face or this image is too small to find an emotion.\n Please try again.");
                if (newHeight < 50 || newWeight < 50)
                {
                    MessageBox.Show("There may be no face in the image.\nSo, please select another image.");
                   
                }
            }
            button_connected.Enabled = false;
            button_skincolor.Enabled = false;
            button2.Enabled = false;
            button1.Enabled = true;
        }


    
    }
}