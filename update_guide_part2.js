const fs = require('fs');

let content = fs.readFileSync('Frontend_Integration_Guide.md', 'utf-8');

const part2Section = `
### 4. قسم الوظائف للمرشحين (Job Searching & Application)
#### أ. صفحة البحث عن الوظائف (Job Feed / Search)
- **المسار (Route):** \`/jobs\`
- **المكونات:**
  - **قائمة الوظائف:** عرض الوظائف النشطة (ينادي \`GET /api/JobPosting\`).
  - **شريط البحث:** بحث بالكلمات المفتاحية (ينادي \`GET /api/JobPosting/search/{searchTerm}\`).
  - **فلاتر (Filters):** تصفية حسب المهارات (\`/api/JobPosting/skill/{id}\`) وحسب نوع العمل (\`/api/JobPosting/type/{type}\`).

#### ب. صفحة تفاصيل الوظيفة (Job Details)
- **المسار (Route):** \`/jobs/{jobId}\`
- **المكونات:**
  - **عرض التفاصيل:** الوصف، المهارات المطلوبة، الراتب.. الخ (ينادي \`GET /api/JobPosting/{id}\`).
  - **زر التقديم (Apply):** لتقديم طلب للوظيفة (ينادي \`POST /api/JobApplication/apply\`). يقوم النظام بالتحقق أولاً إذا كان المتقدم قدم مسبقاً (عبر \`GET /api/JobApplication/check/{candidateId}/{jobId}\`).
  - **زر الحفظ:** (ينادي \`POST /api/Candidates/saved-jobs/{jobId}\`).

#### ج. صفحة مقابلات المرشح (My Interviews)
- **المسار (Route):** \`/candidate/interviews\`
- **المكونات:**
  - **جدول المقابلات:** عرض المقابلات المجدولة الخاصة به (ينادي \`GET /api/Interview/candidate/{candidateId}\`).

---

### 5. قسم إدارة التوظيف للشركات (Recruiter Job Management)
#### أ. صفحة لوحة الوظائف الخاصة بالشركة (Company Jobs)
- **المسار (Route):** \`/recruiter/jobs\`
- **المكونات:**
  - **قائمة الوظائف:** عرض الوظائف التي نشرتها الشركة (ينادي \`GET /api/JobPosting/company/{companyId}\`).
  - **زر إضافة وظيفة:** يوجه لصفحة الإنشاء.
  - **أزرار تحكم لكل وظيفة:** تعديل (يوجه لصفحة التعديل)، حذف (ينادي \`DELETE /api/JobPosting/{id}\`).

#### ب. صفحة إنشاء / تعديل وظيفة (Create/Edit Job)
- **المسار (Route):** \`/recruiter/jobs/new\` و \`/recruiter/jobs/edit/{jobId}\`
- **المكونات:**
  - **فورم الوظيفة:** العنوان، الوصف، المتطلبات..الخ (ينادي \`POST /api/JobPosting?companyId={id}\` للإنشاء أو \`PUT /api/JobPosting/{id}\` للتعديل).

#### ج. لوحة المتقدمين للوظيفة (Job Applications Board)
- **المسار (Route):** \`/recruiter/jobs/{jobId}/applications\`
- **المكونات:**
  - **عرض المتقدمين (Kanban / Table):** عرض المتقدمين مقسمين حسب الحالة (ينادي \`GET /api/JobApplication/job/{jobId}\` أو \`/api/JobApplication/job/{jobId}/status/{status}\`).
  - **تغيير حالة الطلب:** (ينادي \`PUT /api/JobApplication/{id}/status\`).
  - **زر تصدير لملف CSV:** لتحميل قائمة المتقدمين (ينادي \`GET /api/JobApplication/job/{jobId}/export\`).

#### د. إدارة المقابلات (Interview Management)
- **المسار (Route):** \`/recruiter/interviews\`
- **المكونات:**
  - **تحديد موعد مقابلة (Schedule):** فورم لتحديد موعد للمتقدم (ينادي \`POST /api/Interview/schedule\`).
  - **قائمة المقابلات المجدولة:** عرض وتعديل المقابلات (ينادي \`GET /api/Interview/recruiter/interviews\` أو تحديث الموعد عبر \`PUT /api/Interview/{id}\`).
  - **إلغاء مقابلة:** (ينادي \`DELETE /api/Interview/{id}/cancel\`).
`;

// Insert part2Section before the "## 🛠 2. Development Setup" section
if (content.includes("### 3. قسم الشركة ومسؤول التوظيف")) {
    const parts = content.split("## 🛠 2. Development Setup");
    if (parts.length >= 2) {
        // Append part 2 to the UI guide section
        const newContent = parts[0] + part2Section + "\n\n## 🛠 2. Development Setup" + parts.slice(1).join("## 🛠 2. Development Setup");
        fs.writeFileSync('Frontend_Integration_Guide.md', newContent, 'utf-8');
        console.log("Updated Part 2 successfully");
    }
} else {
    console.log("Could not find the end of Part 1");
}
