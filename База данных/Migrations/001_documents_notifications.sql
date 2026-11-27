BEGIN;
-- Stop rather than discard existing duplicates or guess their verification status.
DO $$
BEGIN
    IF EXISTS (SELECT 1 FROM documents GROUP BY user_id HAVING COUNT(*) > 1) THEN
        RAISE EXCEPTION 'Duplicate document sets: reconcile documents by user_id before applying migration';
    END IF;
    IF EXISTS (SELECT 1 FROM documents WHERE status IS NOT NULL AND status NOT IN ('на проверке','одобрено','отклонено')) THEN
        RAISE EXCEPTION 'Unknown document status: review legacy status values before applying migration';
    END IF;
END $$;
UPDATE documents SET status='на проверке' WHERE status IS NULL;
ALTER TABLE documents ALTER COLUMN status SET DEFAULT 'на проверке';
ALTER TABLE documents ALTER COLUMN status SET NOT NULL;
ALTER TABLE documents ADD COLUMN IF NOT EXISTS revision integer NOT NULL DEFAULT 1;
CREATE UNIQUE INDEX IF NOT EXISTS documents_one_set_per_user ON documents(user_id);
DO $$
BEGIN
    IF NOT EXISTS (SELECT 1 FROM pg_constraint WHERE conrelid='documents'::regclass AND conname='documents_status_allowed') THEN
        ALTER TABLE documents ADD CONSTRAINT documents_status_allowed CHECK (status IN ('на проверке','одобрено','отклонено'));
    END IF;
END $$;
CREATE TABLE IF NOT EXISTS staff_users (
    user_id integer PRIMARY KEY REFERENCES users(id) ON DELETE CASCADE
);
CREATE INDEX IF NOT EXISTS notifications_user_date ON notifications(user_id,created_at DESC,id DESC);
COMMIT;
