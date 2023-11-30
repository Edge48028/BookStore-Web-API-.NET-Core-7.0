﻿namespace ModelAndRequest.Cart
{
    public class CartViewModel
    {
        public int bookId { get; set; }
        public string bookName { get; set; }
        public string bookImage { get; set; }
        public float price { get; set; }
        public float sale { get; set; }
        public int quantity { get; set; }
    }
}