using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Mars_Restaurant
{
    public enum FoodType
    {
        maincourse,
        dessert,
        drink
    }

    // This class retrains the name, price, foodType and whether the food is a custom for one food item.
    public class MenuItem
    {
        private string itemName;
        private double itemPrice;
        private bool isCustomItem;
        private FoodType foodType;


        public FoodType getFoodType()
        {
            return foodType;
        }

        public void setFoodType(FoodType ft)
        {
            foodType = ft;
        }

        public string getItemName()
        {
            return itemName;
        }

        public void setItemName(string name)
        {
            itemName = name;
        }

        public double getItemPrice()
        {
            return itemPrice;
        }

        public void setItemPrice(double price)
        {
            itemPrice = price;
        }

        public bool getIsCustomItem()
        {
            return isCustomItem;
        }
        public void setIsCustomItem(bool value)
        {
            isCustomItem = value;
        }

    }
}
