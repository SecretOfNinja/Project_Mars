using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace Mars_Restaurant
{
    // This class retains all of the items for a customer's order or invoice
    public class Order
    {
        private List<MenuItem> itemsInOrder = new List<MenuItem>(); // This is the List container that holds the items ordered.

        private bool orderPaid;

        private double totalBalanceDue;

        public List<MenuItem> deepCopyMenuItems()
        {
            return itemsInOrder;
        }

        public void addItemToOrderItemsInOrder(MenuItem mi)
        {
            itemsInOrder.Add(mi);
        }

        public bool isOrderPaid()
        {
            return orderPaid;
        }

        public void setOrderPaid(bool val)
        {
            orderPaid = val;
        }

        public double getTotalBalanceDue()
        {
            return totalBalanceDue;
        }

        public void clearOrder()
        {
            itemsInOrder.Clear();
        }

        //Clear order and add items from the ListView
        public void addListViewItems(ListView lv)
        {
            int i;
            MenuItem mi;
            ListViewItem lvi;

            itemsInOrder.Clear(); 

            for(i=0; i < lv.Items.Count; i++)
            {
                lvi = lv.Items[i];
                mi = new MenuItem();
                mi.setItemName(lvi.Text);
                if (lvi.SubItems[1].Text == "Main Course")
                    mi.setFoodType(FoodType.maincourse);
                else if(lvi.SubItems[1].Text == "Drink")
                {
                    mi.setFoodType(FoodType.drink);
                }
                else
                {
                    mi.setFoodType(FoodType.dessert);
                }
                mi.setItemPrice(double.Parse(lvi.SubItems[2].Text));
                itemsInOrder.Add(mi);
            }           
        }

        //Clear the ListView and add the order to it
        public void getItemsForListView(ref ListView lv)
        {
            int i;
            ListViewItem lvi;

            lv.Items.Clear();

            for(i=0; i < itemsInOrder.Count; i++)
            {
                lvi = new ListViewItem();
                lvi.Text = itemsInOrder[i].getItemName();

                FoodType ft = itemsInOrder[i].getFoodType();
                if (ft == FoodType.maincourse)
                {
                    lvi.SubItems.Add("Main Course");
                }
                else if (ft == FoodType.drink)
                {
                    lvi.SubItems.Add("Drink");
                }
                else
                {
                    lvi.SubItems.Add("Dessert");
                }
                lvi.SubItems.Add(itemsInOrder[i].getItemPrice().ToString());
                lv.Items.Add(lvi);
            }
        }

        // The constructor for the Order class.
        public Order()
        {
            orderPaid = false;
            totalBalanceDue = 0.0;
        }

    }
}
