import React, { useEffect, useState } from "react";

export default function ApprovePage() {
    const [classes, setClasses] = useState([]);
    const [loading, setLoading] = useState(true);

    // Nếu có JWT thì lấy token ở đây:
    // const token = localStorage.getItem("token");

    // Tạm thời không dùng JWT → admin nhập accId trực tiếp
    const [adminAccId, setAdminAccId] = useState("Acc-00001");

    // ==========================
    // 1) Load tất cả lớp và trạng thái
    // ==========================
    const loadData = async () => {
        setLoading(true);

        const res = await fetch(
            "https://localhost:7287/api/StudentSubjects/class/status"
        );

        const data = await res.json();
        setClasses(data);
        setLoading(false);
    };

    useEffect(() => {
        loadData();
    }, []);

    // ==========================
    // 2) Duyệt lớp
    // ==========================
    const approveClass = async (classId) => {
        if (!adminAccId) {
            alert("Chưa nhập accId của admin!");
            return;
        }

        const confirm = window.confirm(
            `Bạn có chắc muốn DUYỆT lớp ${classId} không?`
        );
        if (!confirm) return;

        const res = await fetch(
            `https://localhost:7287/api/StudentSubjects/class/${classId}/approve?accId=${adminAccId}`,
            {
                method: "PUT",
                headers: {
                    // Nếu dùng JWT thì thêm:
                    // Authorization: `Bearer ${token}`
                },
            }
        );

        const result = await res.json();
        alert(result.message);

        loadData();
    };

    if (loading) return <div>Đang tải...</div>;

    return (
        <div style={{ padding: "20px" }}>
            <h2>Quản lý duyệt điểm</h2>

            {/* Admin nhập accId (nếu chưa dùng JWT) */}
            <div style={{ marginBottom: "20px" }}>
                <label>Nhập accId Admin: </label>
                <input
                    value={adminAccId}
                    onChange={(e) => setAdminAccId(e.target.value)}
                    style={{
                        padding: "5px 10px",
                        marginLeft: "10px",
                        borderRadius: "6px"
                    }}
                />
            </div>

            <table border="1" cellPadding="10" style={{ width: "100%" }}>
                <thead>
                    <tr>
                        <th>Mã lớp</th>
                        <th>Tên lớp</th>
                        <th>Môn học</th>
                        <th>Số lượng</th>
                        <th>Trạng thái</th>
                        <th>Ngày duyệt</th>
                        <th>Người duyệt</th>
                        <th>Hành động</th>
                    </tr>
                </thead>

                <tbody>
                    {classes.map((c, index) => (
                        <tr key={index}>
                            <td>{c.classId}</td>
                            <td>{c.className}</td>
                            <td>{c.subjectId}</td>
                            <td>{c.totalRecords}</td>
                            <td>
                                {c.isApproved ? (
                                    <span style={{ color: "green" }}>ĐÃ DUYỆT</span>
                                ) : (
                                    <span style={{ color: "red" }}>CHƯA DUYỆT</span>
                                )}
                            </td>

                            <td>{c.approvedAt ?? "—"}</td>
                            <td>{c.approvedBy ?? "—"}</td>

                            <td>
                                {!c.isApproved && (
                                    <button
                                        onClick={() => approveClass(c.classId)}
                                        style={{
                                            padding: "6px 12px",
                                            background: "blue",
                                            color: "white",
                                            border: "none",
                                            borderRadius: "5px",
                                        }}
                                    >
                                        Duyệt điểm
                                    </button>
                                )}
                            </td>
                        </tr>
                    ))}
                </tbody>
            </table>
        </div>
    );
}
