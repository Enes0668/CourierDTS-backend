namespace CourierDTS.Dtos
{
    // PUT /api/packages/{id}/assign
    public class AssignCourierRequest
    {
        // null gönderilirse paket havuza geri döner (atama kaldırılır).
        public int? CourierId { get; set; }
    }
}
