import React, { useEffect, useState } from "react";

export default function ApprovePage() {
    const [classes, setClasses] = useState([]);
    const [loading, setLoading] = useState(true);
    const [filter, setFilter] = useState("all"); // all | approved | unapproved

    const token = localStorage.getItem("token");

    // ==========================
    // 1) Load danh sách lớp
    // ==========================
    const loadData = async () => {
        setLoading(true);

        try {
            let url = "https://localhost:7287/api/StudentSubjects/class/status";

            // Nếu chọn lọc → gọi API phù hợp
            if (filter === "approved") {
                url = "https://localhost:7287/api/StudentSubjects/class/status/approved";
            } else if (filter === "unapproved") {
                url = "https://localhost:7287/api/StudentSubjects/class/status/unapproved";
            }

            const res = await fetch(url, {
                headers: { Authorization: `Bearer ${token}` }
            });

            if (!res.ok) {
                alert("Không thể tải dữ liệu.");
                return;
            }

            const data = await res.json();
            setClasses(data);
        } catch (err) {
            alert("Lỗi tải dữ liệu!");
        } finally {
            setLoading(false);
        }
    };

    useEffect(() => {
        loadData();
    }, [filter]); // load lại khi đổi filter

    // ==========================
    // 2) Xử lý duyệt điểm
    // ==========================
    const approveClass = async (classId) => {
        if (!window.confirm(`Bạn muốn DUYỆT lớp ID = ${classId}?`)) return;

        try {
            const res = await fetch(
                `https://localhost:7287/api/StudentSubjects/class/${classId}/approve`,
                {
                    method: "PUT",
                    headers: { Authorization: `Bearer ${token}` }
                }
            );

            if (!res.ok) {
                alert(await res.text());
                return;
            }

            const result = await res.json();
            alert(result.message);

            loadData(); // reload sau khi duyệt
        } catch (err) {
            alert("Duyệt thất bại!");
        }
    };

    if (loading) return <h3>Đang tải dữ liệu...</h3>;

    return (
        <div style={{ padding: "20px" }}>
            <h2>Quản lý duyệt điểm</h2>

            {/* ================== Bộ lọc ================== */}
            <div style={{ marginBottom: 20 }}>
                <button
                    onClick={() => setFilter("all")}
                    style={filterBtn(filter === "all")}
                >
                    Tất cả
                </button>

                <button
                    onClick={() => setFilter("approved")}
                    style={filterBtn(filter === "approved")}
                >
                    Đã duyệt
                </button>

                <button
                    onClick={() => setFilter("unapproved")}
                    style={filterBtn(filter === "unapproved")}
                >
                    Chưa duyệt
                </button>
            </div>

            <table
                style={{
                    width: "100%",
                    borderCollapse: "collapse",
                    background: "#fff"
                }}
            >
                <thead>
                    <tr style={{ background: "#f2f2f2", borderBottom: "2px solid #ccc" }}>
                        <th style={cellHead}>Mã lớp</th>
                        <th style={cellHead}>Tên lớp</th>
                        <th style={cellHead}>Môn học</th>
                        <th style={cellHead}>Số lượng</th>
                        <th style={cellHead}>Trạng thái</th>
                        <th style={cellHead}>Ngày duyệt</th>
                        <th style={cellHead}>Người duyệt</th>
                        <th style={cellHead}>Hành động</th>
                    </tr>
                </thead>

                <tbody>
                    {classes.map((c, index) => (
                        <tr key={index} style={{ borderBottom: "1px solid #ddd" }}>
                            <td style={cell}>{c.classId}</td>
                            <td style={cell}>{c.className}</td>
                            <td style={cell}>{c.subjectId}</td>
                            <td style={cell}>{c.totalRecords}</td>

                            <td style={cell}>
                                {c.isApproved ? (
                                    <span style={{ color: "green", fontWeight: "bold" }}>
                                        ✔ ĐÃ DUYỆT
                                    </span>
                                ) : (
                                    <span style={{ color: "red", fontWeight: "bold" }}>
                                        ✘ CHƯA DUYỆT
                                    </span>
                                )}
                            </td>

                            <td style={cell}>
                                {c.approvedAt
                                    ? new Date(c.approvedAt).toLocaleDateString("vi-VN")
                                    : "—"}
                            </td>

                            <td style={cell}>{c.approvedBy ?? "—"}</td>

                            <td style={cell}>
                                {!c.isApproved && (
                                    <button
                                        onClick={() => approveClass(c.classId)}
                                        style={btnApprove}
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

/* ================= CSS ================= */

const cellHead = {
    padding: "10px",
    textAlign: "center",
    fontWeight: "bold",
    borderBottom: "2px solid #ddd"
};

const cell = {
    padding: "10px",
    textAlign: "center"
};

const btnApprove = {
    padding: "6px 12px",
    background: "blue",
    color: "white",
    border: "none",
    borderRadius: "5px",
    cursor: "pointer"
};

const filterBtn = (active) => ({
    padding: "8px 15px",
    marginRight: 10,
    borderRadius: 5,
    cursor: "pointer",
    border: "1px solid #ccc",
    background: active ? "#007bff" : "#f2f2f2",
    color: active ? "white" : "black",
    fontWeight: active ? "bold" : "normal"
});
