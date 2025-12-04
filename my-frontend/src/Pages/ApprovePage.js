import React, { useEffect, useState } from "react";

export default function ApprovePage() {
    const [classes, setClasses] = useState([]);
    const [loading, setLoading] = useState(true);

    // Lấy token từ localStorage
    const token = localStorage.getItem("token");

    // ==========================
    // 1) Load danh sách lớp
    // ==========================
    const loadData = async () => {
        setLoading(true);

        const res = await fetch("https://localhost:7287/api/StudentSubjects/class/status", {
            headers: {
                "Authorization": `Bearer ${token}`
            }
        });

        if (!res.ok) {
            alert("Không thể tải dữ liệu. Token có thể hết hạn.");
            return;
        }

        const data = await res.json();
        setClasses(data);
        setLoading(false);
    };

    useEffect(() => {
        loadData();
    }, []);

    // ==========================
    // 2) Gửi yêu cầu DUYỆT LỚP
    // ==========================
    const approveClass = async (classId) => {
        const confirm = window.confirm(`Bạn có chắc muốn DUYỆT lớp ${classId} không?`);
        if (!confirm) return;

        const res = await fetch(
            `https://localhost:7287/api/StudentSubjects/class/${classId}/approve`,
            {
                method: "PUT",
                headers: {
                    "Authorization": `Bearer ${token}`
                }
            }
        );

        if (!res.ok) {
            const error = await res.text();
            alert("Lỗi khi duyệt lớp: " + error);
            return;
        }

        const result = await res.json();
        alert(result.message);

        loadData();
    };

    if (loading) return <div>Đang tải dữ liệu...</div>;

    return (
        <div style={{ padding: "20px" }}>
            <h2>Quản lý duyệt điểm</h2>

            <table border="1" cellPadding="10" style={{ width: "100%", marginTop: "20px" }}>
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
                                            cursor: "pointer"
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
