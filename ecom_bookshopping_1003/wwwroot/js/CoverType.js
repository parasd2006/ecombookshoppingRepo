var dataTable;
$(document).ready(function () {
    loadDataTable();
})
function loadDataTable() {
    dataTable = $("#tablData").DataTable({
        "ajax": {
            "url": "/Admin/CoverType/GetAll",

        },
        "columns": [

            {
                "data": "id",
                "render": function (data) {
                    return `
                    <div class="text-center ">
                    <a href="/Admin/CoverType/Upsert/${data}" class="btn btn-info">
                    <i class="fas fa-edit"></i>
                    </a>
                    </div>
                    `;
                }
            },
            { "data": "name", "width": "50%" },
            {
                "data": "id",
                "render": function (data) {
                    return `
                    <div class="text-center">
                    <a onclick="Delete('/Admin/CoverType/Delete/${data}')" class="btn btn-danger">
                    <i class="fas fa-trash"></i>
                    </a>
                    </div>
                    `;
                }
            }
        ]
    })
}
function Delete(url) {
    swal({
        title: "Want to Delete Data?",
        text: "Delete Information",
        icon: "warning",
        buttons: true,
        dangerModel:true
    }).then((willDelete) => {
        if (willDelete) {
            $.ajax({
                url: url,
                type:"DELETE",
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