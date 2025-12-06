import { useEffect, useState } from "react";

export default function TeacherClasses() {
  const [classes, setClasses] = useState([]);
  const [loading, setLoading] = useState(true);

  useEffect(() => {
    const token = localStorage.getItem("token");
    if (!token) return;

    fetch("https://localhost:7287/api/Teachers/classes/current", {
      method: "GET",
      headers: {
        Authorization: `Bearer ${token}`,
        "Content-Type": "application/json"
      }
    })
      .then(res => {
        if (!res.ok) throw new Error("Không thể tải danh sách lớp!");
        return res.json();
      })
      .then(data => {
        setClasses(data);
        setLoading(false);
      })
      .catch(err => {
        console.error(err);
        setLoading(false);
      });
  }, []);

  if (loading) return <h3>Đang tải danh sách lớp...</h3>;

  return (
    <div style={{ padding: 20 }}>
      <h2>Các lớp đang giảng dạy</h2>

      <table style={{ width: "100%", borderCollapse: "collapse", marginTop: 16 }}>
        <thead>
          <tr style={{ background: "#f2f2f2" }}>
            <th style={cell}>Mã</th>
             <th style={cell}>Mã lớp</th>
            <th style={cell}>Tên lớp</th>
            <th style={cell}>Phòng học</th>
            <th style={cell}>Thứ</th>
            <th style={cell}>Tiết bắt đầu</th>
            <th style={cell}>Tiết kết thúc</th>
          </tr>
        </thead>

        <tbody>
          {classes.length === 0 ? (
            <tr><td colSpan="6" style={cell}>Không có lớp nào</td></tr>
          ) : (
            classes.map((c, i) => (
              <tr key={i}>
                <td style={cell}>{c.id}</td>
                <td style={cell}>{c.classId}</td>
                <td style={cell}>{c.className}</td>
                <td style={cell}>{c.room}</td>
                <td style={cell}>{c.dayOfWeek}</td>
                <td style={cell}>{c.startPeriod}</td>
                <td style={cell}>{c.endPeriod}</td>
              </tr>
            ))
          )}
        </tbody>
      </table>
    </div>
  );
}

const cell = {
  padding: "10px",
  borderBottom: "1px solid #ddd",
  textAlign: "center"
};
