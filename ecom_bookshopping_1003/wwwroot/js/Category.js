var dataTable;
$(document).ready(function () {
    $.fn.DataTable.ext.pager.numbers_length = 5;

    loadDataTable();
})
function loadDataTable() {
    dataTable=$('#tblData').DataTable(
        {
            "ajax": {
                "url": "/Admin/Category/GetAll"
            },
           
            "pageLength": 2,
            "lengthMenu": [2, 4, 6, 8, 10],
           
            "columns": [

               
                {
                    "data": "id",
                    "render": function (data) {
                        return `
                        <div class="text-center">
                        <a href="/Admin/Category/Upsert/${data}" class="btn btn-info">
                        <i class="fas fa-edit"></i>
                        </a>
                         <a class="btn btn-danger" onclick="Delete('/Admin/Category/Delete/${data}')">
                        <i class="fas fa-trash"></i>
                        </a>
                        </div>
                        `;
                    }
                },
                
                 { "data": "name", "width": "70%" }
            ]
        })
}
function Delete(url) {
   // alert(url);
    swal({
        title: "want to delete data",
        text: "Delete information",
        icon: "warning",
        buttons: true,
        dangerModel:true,
    }).then((willdelete) => {
        if (willdelete) {
            $.ajax({
                url: url,
                type: "DELETE",
                success: function (data) {
                    if (data.success) {
                        toastr.success(data.message);
                        dataTable.ajax.reload();

                    } else {
                        toastr.error(data.message);
                    }
                }


            })
        }
    })
}