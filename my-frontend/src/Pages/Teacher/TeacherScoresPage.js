import React, { useEffect, useState } from "react";

export default function TeacherScoresPage() {
    const [classes, setClasses] = useState([]);   // Danh sách lớp (dropdown)
    const [selectedClassId, setSelectedClassId] = useState(""); // ID lớp được chọn

    const [scores, setScores] = useState([]);     // Danh sách điểm
    const [loading, setLoading] = useState(false);

    const API_CLASS = "https://localhost:7287/api/Teachers/classes/current";
    const API_SCORE = "https://localhost:7287/api/StudentSubjects";

    // ======================================================
    // 1) LOAD DANH SÁCH LỚP
    // ======================================================
    const loadClasses = async () => {
        try {
            const token = localStorage.getItem("token");

            const res = await fetch(API_CLASS, {
                headers: { Authorization: `Bearer ${token}` }
            });

            const data = await res.json();
            setClasses(data);

            // Nếu có lớp → tự chọn lớp đầu tiên
            if (data.length > 0) {
                setSelectedClassId(data[0].id);
            }
        } catch (err) {
            alert("Lỗi tải danh sách lớp");
        }
    };

    // ======================================================
    // 2) LOAD DANH SÁCH ĐIỂM
    // ======================================================
    const loadScores = async () => {
        if (!selectedClassId) return;

        setLoading(true);
        try {
            const token = localStorage.getItem("token");

            const res = await fetch(`${API_SCORE}/class/${selectedClassId}`, {
                headers: { Authorization: `Bearer ${token}` }
            });

            if (!res.ok) throw new Error(await res.text());

            const data = await res.json();
            setScores(data);
        } catch (err) {
            alert("Lỗi tải điểm: " + err.message);
        } finally {
            setLoading(false);
        }
    };

    // ======================================================
    // 3) CẬP NHẬT TRONG STATE
    // ======================================================
    const updateLocal = (studentId, field, value) => {
        setScores(prev =>
            prev.map(s => s.studentId === studentId ? { ...s, [field]: value } : s)
        );
    };

    // ======================================================
    // 4) LƯU ĐIỂM 1 SINH VIÊN
    // ======================================================
    const saveScore = async (item) => {
        try {
            const token = localStorage.getItem("token");

            const res = await fetch(
                `${API_SCORE}/student-subject/${item.studentId.trim()}/${item.subjectId.trim()}/${item.semesterId.trim()}`,
                {
                    method: "PUT",
                    headers: {
                        "Content-Type": "application/json",
                        Authorization: `Bearer ${token}`
                    },
                    body: JSON.stringify({
                        point1: item.point1 ?? null,
                        point2: item.point2 ?? null,
                        point3: item.point3 ?? null
                    })
                }
            );

            if (!res.ok) throw new Error(await res.text());

            alert("Đã lưu điểm!");
            loadScores();
        } catch (err) {
            alert("Lỗi khi lưu: " + err.message);
        }
    };

    // ======================================================
    // 5) GỬI TOÀN LỚP
    // ======================================================
    const submitClass = async () => {
        if (!window.confirm("Bạn chắc muốn gửi toàn bộ điểm lớp này?")) return;

        try {
            const token = localStorage.getItem("token");

            const res = await fetch(`${API_SCORE}/class/${selectedClassId}/submit`, {
                method: "PUT",
                headers: { Authorization: `Bearer ${token}` }
            });

            if (!res.ok) throw new Error(await res.text());

            alert("Đã gửi lớp!");
            loadScores();
        } catch (err) {
            alert("Lỗi khi gửi lớp: " + err.message);
        }
    };

    // ======================================================
    // 6) LOAD LỚP KHI VÀO TRANG
    // ======================================================
    useEffect(() => {
        loadClasses();
    }, []);

    // ======================================================
    // 7) LOAD ĐIỂM KHI CHỌN LỚP
    // ======================================================
    useEffect(() => {
        loadScores();
    }, [selectedClassId]);

    return (
        <div style={{ padding: "20px" }}>
            <h2>Nhập điểm</h2>

            {/* ======================= */}
            {/* DROPDOWN CHỌN LỚP */}
            {/* ======================= */}
            <div style={{ marginBottom: 20 }}>
                <label style={{ marginRight: 10 }}>Chọn lớp:</label>
                <select
                    value={selectedClassId}
                    onChange={(e) => setSelectedClassId(e.target.value)}
                    style={{ padding: 6 }}
                >
                    {classes.map(c => (
                        <option key={c.id} value={c.id}>
                            {c.classId} — {c.className}
                        </option>
                    ))}
                </select>
            </div>

            {/* BUTTON */}
            <button
                onClick={submitClass}
                style={{
                    background: "green",
                    color: "white",
                    padding: "8px 14px",
                    borderRadius: 5
                }}
            >
                Gửi điểm lớp
            </button>

            <button
                onClick={loadScores}
                style={{ marginLeft: 10, padding: "8px 14px", borderRadius: 5 }}
            >
                Làm mới
            </button>

            <br /><br />

            {loading ? (
                <h3>Đang tải dữ liệu...</h3>
            ) : (
                <table style={{ width: "100%", borderCollapse: "collapse" }}>
                    <thead>
                        <tr style={{ background: "#f0f0f5" }}>
                            <th className="td">Mã SV</th>
                            <th className="td">Tên SV</th>
                            <th className="td">Điểm 1</th>
                            <th className="td">Điểm 2</th>
                            <th className="td">Điểm 3</th>
                            <th className="td">Tổng</th>
                            <th className="td">Trạng thái</th>
                            <th className="td">Lưu</th>
                        </tr>
                    </thead>

                    <tbody>
                        {scores.map((item, idx) => (
                            <tr key={idx}>
                                <td className="td">{item.studentId}</td>
                                <td className="td">{item.studentName}</td>

                                <td className="td">
                                    <input
                                        type="number"
                                        step="0.1"
                                        value={item.point1 ?? ""}
                                        onChange={(e) =>
                                            updateLocal(item.studentId, "point1", e.target.value)
                                        }
                                        className="input-td"
                                    />
                                </td>

                                <td className="td">
                                    <input
                                        type="number"
                                        step="0.1"
                                        value={item.point2 ?? ""}
                                        onChange={(e) =>
                                            updateLocal(item.studentId, "point2", e.target.value)
                                        }
                                        className="input-td"
                                    />
                                </td>

                                <td className="td">
                                    <input
                                        type="number"
                                        step="0.1"
                                        value={item.point3 ?? ""}
                                        onChange={(e) =>
                                            updateLocal(item.studentId, "point3", e.target.value)
                                        }
                                        className="input-td"
                                    />
                                </td>

                                <td className="td">{item.pointTotal ?? "-"}</td>
                                <td className="td">
                                    {item.isApproved === 1 ? "Đã duyệt" : "Chưa duyệt"}
                                </td>

                                <td className="td">
                                    <button
                                        onClick={() => saveScore(item)}
                                        style={{ padding: "6px 10px", borderRadius: 5 }}
                                    >
                                        Lưu
                                    </button>
                                </td>
                            </tr>
                        ))}
                    </tbody>
                </table>
            )}

            <style>{`
                .td {
                    padding: 10px;
                    border-bottom: 1px solid #ddd;
                    text-align: center;
                }
                .input-td {
                    width: 70px;
                    padding: 5px;
                    text-align: center;
                }
            `}</style>
        </div>
    );
}
