const fs = require('fs');

let content = fs.readFileSync('Frontend_Integration_Guide.md', 'utf-8');

const part3Section = `
### 6. قسم التقييمات والذكاء الاصطناعي (Assessments & AI)
#### أ. صفحة إنشاء تقييم (Create Assessment) - للشركات
- **المسار (Route):** \`/recruiter/assessments/new\`
- **المكونات:**
  - **توليد عبر الذكاء الاصطناعي:** زر (Generate via AI) لاختيار وظيفة وعدد الأسئلة وسيقوم النظام بتوليدها (ينادي \`POST /api/Assessments/job/{jobId}/generate\`).
  - **إنشاء يدوي:** فورم لكتابة الأسئلة يدوياً وإضافتها لوظيفة (ينادي \`POST /api/Assessments\`).

#### ب. صفحة أداء التقييم (Take Assessment) - للمرشحين
- **المسار (Route):** \`/candidate/assessments/{assessmentId}\`
- **المكونات:**
  - **بدء التقييم:** زر "Start" لجلب بيانات الأسئلة (ينادي \`POST /api/Assessments/{id}/start\`).
  - **واجهة الأسئلة:** عرض الأسئلة الواحد تلو الآخر أو في صفحة واحدة، مع مؤقت (إن وجد).
  - **تسليم التقييم:** زر "Submit" يقوم بإرسال إجابات المرشح (ينادي \`POST /api/Assessments/{id}/submit\`).

---

### 7. لوحة التحكم والإحصائيات (Dashboards)
#### أ. لوحة تحكم المرشح (Candidate Dashboard)
- **المسار (Route):** \`/candidate/dashboard\`
- **المكونات:**
  - **الإحصائيات الرئيسية:** إجمالي التقديمات، المقابلات القادمة، الوظائف المحفوظة (ينادي \`GET /api/Dashboards/candidate\`).

#### ب. لوحة تحكم الشركة (Company/Recruiter Dashboard)
- **المسار (Route):** \`/recruiter/dashboard\`
- **المكونات:**
  - **الإحصائيات الرئيسية:** إجمالي الوظائف النشطة، عدد المتقدمين الإجمالي، المقابلات القادمة للشركة (ينادي \`GET /api/Dashboards/company/{companyId}\`).

---

### 8. قسم الإشعارات والتواصل المباشر (Notifications & Real-time)
#### أ. مكون الإشعارات (Notification Dropdown / Bell Icon)
- **المكان:** شريط التنقل العلوي (Navbar).
- **المكونات:**
  - **رقم الإشعارات غير المقروءة:** (ينادي \`GET /api/Notifications/unread-count\`).
  - **قائمة الإشعارات:** جلب آخر الإشعارات (ينادي \`GET /api/Notifications\`).
  - **تحديد كمقروء:** زر عند كل إشعار (ينادي \`PATCH /api/Notifications/{id}/mark-read\`) وزر للكل (ينادي \`PATCH /api/Notifications/read-all\`).
  - **الربط المباشر (SignalR):** الاستماع لـ \`/hubs/notifications\` لاستقبال الإشعارات الجديدة فورياً دون إعادة تحميل الصفحة (تفاصيل أكثر في قسم Real-Time بالأسفل).
`;

// Insert part3Section before the "## 🛠 2. Development Setup" section
if (content.includes("### 5. قسم إدارة التوظيف للشركات")) {
    const parts = content.split("## 🛠 2. Development Setup");
    if (parts.length >= 2) {
        // Append part 3 to the UI guide section
        const newContent = parts[0] + part3Section + "\n\n## 🛠 2. Development Setup" + parts.slice(1).join("## 🛠 2. Development Setup");
        fs.writeFileSync('Frontend_Integration_Guide.md', newContent, 'utf-8');
        console.log("Updated Part 3 successfully");
    }
} else {
    console.log("Could not find the end of Part 2");
}
