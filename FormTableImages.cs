using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace Mars_Restaurant
{
    // This class does the work of displaying the dining table images to
    // the user so that he can select one.
    public partial class FormTableImages : Form
    {       
        DoubleBufferedWindow _parentForm;
        int _imgIndex=0;
        int _numberOfImages;
        ImageList _imgLst; 
        public int _selectedImage { get; set; }

        // The constructor for FromTableImages.    This class displays
        // the table images for user selection.
        // The parameters passed in include the DoubleBufferedWindow parentForm,
        // and the imgLst.  imgList holds ImageList collection of images to be
        // displayed
        public FormTableImages(DoubleBufferedWindow parentForm,ref ImageList imgLst)
        {
            InitializeComponent();
            _parentForm = parentForm;
            _imgLst = imgLst;
            pbxImages.Image = _imgLst.Images[0];
            _numberOfImages = _imgLst.Images.Count;
            _selectedImage = -1;

            // Fix the window size
            this.MinimumSize = new Size(this.Width, this.Height);
            this.MaximumSize = new Size(this.Width, this.Height);
        }

        // Go to previous image in _imgLst
        private void btnPrev_Click(object sender, EventArgs e)
        {
            if (_imgIndex > 0)
            {
                _imgIndex--;
                pbxImages.Image = _imgLst.Images[_imgIndex];
            }           
        }

        // Go to next image in _imgLst
        private void btnNext_Click(object sender, EventArgs e)
        {
            if (_imgIndex < _numberOfImages-1)
            {
                _imgIndex++;
                pbxImages.Image = _imgLst.Images[_imgIndex];
            }
        }

        // Exit the form without selecting an image
        private void btnCancel_Click(object sender, EventArgs e)
        {
            _selectedImage = -1;
            this.Close();
        }

        // Exit the form after selecting the current image
        // Update the database with the newly selected table style
        private void btnOk_Click(object sender, EventArgs e)
        {
            //updateTableImages
            _parentForm.updateTableImages(_imgIndex); // _imgIndex is the current selected index
            _parentForm.updateTableStyleInDatabase();
            this.Close();
        }
    }
}
