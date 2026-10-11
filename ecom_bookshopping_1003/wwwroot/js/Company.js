var dataTable;
$(document).ready(function () {
    loadDataTable();
})

function loadDataTable() {
    dataTable = $('#tblData').DataTable({
        "ajax": {
            url: "/Admin/Company/GetAll"
        },
        columns: [
            { "data": "name", "width": "10%" },
            { "data": "streetAddress", "width": "15%" },
            { "data": "city", "width": "10%" },
            { "data": "state", "width": "10%" },
            { "data": "postalCode", "width": "10%" },
            { "data": "phoneNumber", "width": "10%" },
            { "data": "isAuthorizedCompany", "width": "20%" },
            {
                "data": "id",
                "render": function (data) {
                    return `
                    <div class="text-center">
                    <a href="/Admin/Company/Upsert/${data}" class="btn btn-info ">
                    <i class="fas fa-edit"></i></a>
                    <a onclick="Delete('/Admin/Company/Delete/${data}')" class="btn btn-danger">
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
        dangerModel: true
    }).then((willDelete) => {
        if (willDelete) {
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