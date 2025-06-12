$(document).ready(function () {
    ShowCount();

    $('body').on('click', '.btnAddtoCart', function (e) {
        e.preventDefault();

        var id = $(this).data('id');
        var quantity = 1;
        var tQuantity = $('#quantity_value').text();

        if (tQuantity !== '') {
            quantity = parseInt(tQuantity);
        }

        alert(id + " " + quantity);

        $.ajax({
            url: '/ShoppingCart/AddtoCart',
            type: 'POST',
            data: { id: id, quantity: quantity },
            success: function (rs) {
                if (rs.Success) {
                    $('#checkout_items').html(rs.Count);
                    alert(rs.Msg);
                }
            },
            error: function () {
                // Handle the error here
                alert("An error occurred");
            }
        });
    });
    $('body').on('click', '.btnDelete', function (e) {
        e.preventDefault();
        var id = $(this).data('id');
        var conf = confirm('Bạn có chắc muốn xóa sản phẩm này khỏi giỏ hàng không?');
        if (conf == true) {
            $.ajax({
                url: '/shoppingcart/Delete',
                type: 'POST',
                data: { id: id},
                success: function (rs) {
                    if (rs.Success) {
                        $('#checkout_items').html(rs.Count);
                        $('#trow_' + id).remove();
                        LoadCart();
                    }
                }
            });

        }
    });

    //$('body').on('click', '.btnUpdate', function (e) {
    //    e.preventDefault();
    //    var id = $(this).data('id');
    //    var quantity = $('#Quantity_' + id).val();
    //    Update(id, quantity);
    //});

    $(document).ready(function () {
        $('.btnDeleteAll').click(function () {
            var conf = confirm('Bạn có chắc muốn xóa hết sản phẩm trong giỏ hàng không?');
            if (conf == true) {
                $.ajax({
                    url: '/shoppingcart/DeleteAll',
                    type: 'POST',
                    dataType: 'json',
                    success: function (rs) {
                        if (rs.Success) {
                            DeleteAll();
                            LoadCart();
                        }
                    }
                });
            }
        });
    });

    //$('body').on('click', '.btnDeleteAll', function (e) {
    //    e.preventDefault();
    //    var conf = confirm('Bạn có chắc muốn xóa hết sản phẩm trong giỏ hàng không?');
    //    //if (conf == true) {
    //    //    DeleteAll();
    //    //}
    //});
    $(document).ready(function () {
        $('body').on('click', '.btnUpdate', function (e) {
            e.preventDefault();
            var id = $(this).data('id');
            var quantity = 1; // Set the desired quantity here or retrieve it from an input element

            // Example: Retrieve quantity from an input field with id 'Quantity_<ProductId>'
            var inputSelector = '#Quantity_' + id; //Quantity_SP11186
            if ($(inputSelector).length) {
                quantity = parseInt($(inputSelector).val(), 10);
            }

            Update(id, quantity);
            LoadCart();
        });
    });

});



function ShowCount(){
    $.ajax({
        url: '/shoppingcart/ShowCount',
        type: 'GET',
        success: function (rs) {
            if (rs.Success) {
                $('#checkout_items').html(rs.Count);
            }
        }
    });
}
function DeleteAll() {
    $.ajax({
        url: '/shoppingcart/DeleteAll',
        type: 'POST',
        success: function (rs) {
            if (rs.Success) {
                LoadCart();
            }
        }
    });
}

function Update(id, quantity) {
    $.ajax({
        url: '/Shoppingcart/Update',
        type: 'POST',
        data: { id: id, quantity: quantity },
        success: function (rs) {
            if (rs.Success) {
                $('#checkout_items').html(rs.Count);
                alert('Quantity updated successfully!');
            } else {
                alert('Failed to update quantity.');
            }
        },
        error: function () {
            alert('An error occurred while updating the quantity.');
        }
    });
}




function LoadCart() {
    var url = '/shoppingcart/Partial_Item_Cart?t=' + new Date().getTime();
    $.ajax({
        url: url,
        type: 'GET',
        success: function (rs) {
            if (rs.Success) {
                $('#load_data').html(rs.Count);
            }
        }
    });
}




